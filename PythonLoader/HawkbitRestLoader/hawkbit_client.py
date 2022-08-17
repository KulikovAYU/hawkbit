from __future__ import annotations
import datetime
import os
import time
from Hawkbit.utils import *
from datetime import datetime
from urllib.parse import urlparse
import requests
from requests import Session
from requests.auth import AuthBase
from Hawkbit.actions_impl import *
from Hawkbit.hawkbit_response import *


class EventHandler:
    def on_no_actions(self):
        print("No actions from hawkBit")

    def on_deployment_action(self, deployment_data: DeploymentBase) -> Response:
        print(">> Get Deployment base request")
        print("id: ", deployment_data.id, " update: ", deployment_data.update_type)
        print(f" download: {deployment_data.download_type} inWindow: {deployment_data.in_maintenance_window}")

        response = Response()
        response.add_detail("Printed deployment base info")
        print(" + CHUNKS:")
        for chunk in deployment_data.chunks:
            print("  part: ", chunk._part, "\n")
            print(" name: ", chunk._name, " version: ", chunk._version, "\n")
            print(" + ARTIFACTS:")
            for artifact in chunk._artifacts:
                print("   filename: ", artifact._file_name, " size: ", artifact._file_size, "\n")
                print("   md5: ", artifact._file_hashes._md5, "\n")
                print("   sha1: ", artifact._file_hashes._sha1, "\n")
                print("   sha1: ", artifact._file_hashes._sha256, "\n")
                response.add_detail(artifact._file_name + " described. Starting download ...")
                print("  .. downloading ", artifact._file_name, "...\n")
                download_path = os.getcwd()
                full_path = os.path.join(download_path, artifact._file_name)
                artifact.download_to(full_path)
                response.add_detail("Downloaded " + artifact._file_name)
                print("[OK]")
            print("+ ---------------------------")

        response.add_detail("Work done. Sending response") \
            .set_ignore_sleep() \
            .set_execution(Response.Execution.CLOSED) \
            .set_finished(Response.Finished.SUCCESS) \
            .set_response_delivery_listener(ResponseDeliveryListener())

        return response

    def on_cancel_action(self) -> Response:
        return Response()

    def on_config_request(self):
        print(">> Sending Config Data")
        response = ConfigResponse()
        response.add_data("some", "config1") \
            .add_data("some1", "new config") \
            .add_data("some2", "RITMS123") \
            .add_data("some3", "TES_TEST_TEST") \
            .set_ignore_sleep()

        return response


class Actions(Enum):
    GET_CONFIG_DATA = 0
    CANCEL_ACTION = 1
    DEPLOYMENT_BASE = 2
    NONE = 3


class PollingData:
    def __init__(self):
        self._sleep_time = 0
        self._action = Actions.NONE
        self._follow_uri = ""

    def get_sleep_time(self):
        return self._sleep_time

    def get_action(self) -> Actions:
        return self._action

    def get_follow_uri(self):
        return self._follow_uri

    @staticmethod
    def from_string(data) -> PollingData:
        data = json.loads(data.text)
        object_pooling = PollingData()
        if "config" in data:
            config = data["config"]
            if "sleep" in config["polling"]:
                sleep_time = config["polling"]
                sleep_time = sleep_time["sleep"]
                parsed_time = datetime.strptime(sleep_time, '%H:%M:%S')
                sleep_time = (parsed_time.hour * 3600 + parsed_time.minute * 60 + parsed_time.second) * 1000
                object_pooling._sleep_time = sleep_time

        if "_links" not in data:
            return object_pooling

        links = data["_links"]
        # there`re 3 variants of actions: configData, deploymentBase, cancelAction
        # priority: configData, (cancelAction/deploymentBase)
        if "configData" in links:
            object_pooling._follow_uri = links["configData"]
            object_pooling._action = Actions.GET_CONFIG_DATA
        elif "cancelAction" in links:
            object_pooling._follow_uri = links["cancelAction"]
            object_pooling._action = Actions.CANCEL_ACTION
        elif "deploymentBase" in links:
            object_pooling._follow_uri = links["deploymentBase"]
            object_pooling._action = Actions.DEPLOYMENT_BASE

        return object_pooling


class ApiKeyAuth(AuthBase):
    """Implements a custom authentication scheme."""

    def __init__(self, token):
        self.token = token

    def __call__(self, req):
        """Attach an API token to a custom auth header."""
        req.headers['Authorization'] = f'{self.token}'
        return req


class Client:
    def __init__(self):
        self._hawkbit_uri = ""
        self._hawkbit_net_loc = ""
        self._token = ""
        self._handler = None
        self._pollingTimeout = 0
        self._ignore_sleep = False
        self._current_sleep_time = 0
        self._headers = {}

    def run(self):
        if self._hawkbit_uri == "":
            raise ValueError("endpoint or AuthErrorHandler is not set")

        print("hawkBit-py client started...")
        while True:
            self.do_poll()
            if self._ignore_sleep == False and self._current_sleep_time > 0:
                seconds = (self._current_sleep_time / 1000) % 60
                time.sleep(seconds)

    def do_poll(self):
        resp = self.retry_handler(self._hawkbit_uri, self.get_request)
        poling_data = PollingData.from_string(resp)
        if poling_data.get_sleep_time() > 0:
            self._current_sleep_time = poling_data.get_sleep_time()
        else:
            self._current_sleep_time = self._pollingTimeout

        follow_uri = poling_data.get_follow_uri()

        action = poling_data.get_action()
        if action == Actions.NONE:
            self._handler.on_no_actions()
            return
        elif action == Actions.GET_CONFIG_DATA:
            self.follow_config_data(follow_uri["href"])
            return
        elif action == Actions.CANCEL_ACTION:
            self.follow_cancel_action(follow_uri["href"])
            return
        elif action == Actions.DEPLOYMENT_BASE:
            self.follow_deployment_base(follow_uri["href"])
            return

    def retry_handler(self, uri, send_request_callback) -> Response:
        try:
            return self.wrapped_request(uri, send_request_callback)
        except Exception as e:
            raise e

    def wrapped_request(self, uri, send_request_callback):
        headers = {'Content-Type': 'application/json'}
        for key, value in self._headers:
            headers.update({key: value})

        session = requests.Session()
        session.auth = ApiKeyAuth(self._token)
        session.headers = headers
        return send_request_callback(uri, session)

    def follow_config_data(self, uri):
        request = self._handler.on_config_request()
        request_data = request.get_data()
        if not bool(request_data):
            return

        cli_resp = Response()
        cli_resp.set_finished(Response.Finished.SUCCESS). \
            set_execution(Response.Execution.CLOSED)

        action_id = -1
        json_doc = json.loads(self.fill_response_document(cli_resp, action_id))
        json_doc["data"] = request_data
        buf = json.dumps(json_doc)

        resp = self.retry_handler(uri, send_request_callback=lambda follow_uri, session: session.put(
            url=follow_uri, data=buf))

        self._ignore_sleep = request.is_ignore_sleep()

    def follow_cancel_action(self, uri):
        resp = self.retry_handler(uri, self.get_request)
        cancel_action = CancelAction.from_string(resp)
        cli_resp = Response()
        if self._handler is not None:
            cli_resp = self._handler.on_cancel_action(cancel_action)

            action_id = cancel_action.id
            buf = self.fill_response_document(cli_resp, action_id)

        try:
            resp = self.retry_handler(uri, send_request_callback=lambda follow_uri, session: session.post(
                url=(self.format_feedback_path(
                    follow_uri)), data=buf))

            # TODO: check status code
            if cli_resp.response_delivery_listener is not None:
                cli_resp.response_delivery_listener.on_successful_delivery()

            return resp
        except Exception as e:
            if cli_resp.response_delivery_listener is not None:
                cli_resp.response_delivery_listener.on_error()
            else:
                raise e

        self._ignore_sleep = cli_resp.ignore_sleep

    def follow_deployment_base(self, uri):
        resp = self.retry_handler(uri, self.get_request)
        deployment_base = DeploymentBase.from_body(resp, self)

        cli_resp = Response()
        if self._handler is not None:
            cli_resp = self._handler.on_deployment_action(deployment_base)

        action_id = deployment_base.id
        buf = self.fill_response_document(cli_resp, action_id)

        try:
            resp = self.retry_handler(uri, send_request_callback=lambda follow_uri, session: session.post(
                url=(self.format_feedback_path(
                    follow_uri)), data=buf))

            # TODO: check status code
            if cli_resp.response_delivery_listener is not None:
                cli_resp.response_delivery_listener.on_successful_delivery()

            return resp
        except Exception as e:
            if cli_resp.response_delivery_listener is not None:
                cli_resp.response_delivery_listener.on_error()
            else:
                raise e

    def format_feedback_path(self, uri):
        hawkbit_end_point = urlparse(uri)
        path = self._hawkbit_net_loc + hawkbit_end_point.path + "/feedback"
        return path

    def fill_response_document(self, response, action_id):
        json_response = {}
        json_details = response._details

        sub_json_result = {"result": {"finished": Utils.finished_to_string(response.finished)},
                           "execution": Utils.execution_to_string(response.execution),
                           "details": json_details}

        json_response["status"] = sub_json_result
        if int(action_id) >= 0:
            json_response["id"] = action_id

        document = json.dumps(json_response)
        return document

    def download_to(self, download_uri, path):
        self.retry_handler(download_uri,
                           send_request_callback=lambda uri, session: self.get_request_file(uri=uri, session=session,
                                                                                            path=path))

    @staticmethod
    def get_request(uri, session: Session) -> Response:
        payload = {}
        return session.get(uri, data=payload)

    @staticmethod
    def get_request_file(uri, session: Session, path):
        get_response = session.get(uri, stream=True)
        with open(path, 'wb') as f:
            for chunk in get_response.iter_content(chunk_size=1024):
                if chunk:  # filter out keep-alive new chunks
                    f.write(chunk)


class DefaultClientBuilder:
    def __init__(self):
        self._hawkbit_uri = ""
        self._hawkbit_net_loc = ""
        self._token = ""
        self._handler = None
        self._polling_timeout = None

    def set_hawkbit_endpoint(self, endpoint, controller_id, tenant="default"):
        hawkbit_end_point = urlparse(endpoint)
        self._hawkbit_net_loc = hawkbit_end_point.scheme + "://" + hawkbit_end_point.netloc
        self._hawkbit_uri = self._hawkbit_net_loc + "/" + tenant + "/controller/v1/" + controller_id
        return self

    def set_gateway_token(self, token):
        self._token = token
        return self

    def set_event_handler(self, handler):
        self._handler = handler
        return self

    def set_default_polling_timeout_in_ms(self, polling_timeout):
        self._polling_timeout = polling_timeout
        return self

    def build(self):
        cli = Client()
        cli._hawkbit_uri = self._hawkbit_uri
        cli._hawkbit_net_loc = self._hawkbit_net_loc
        cli._token = self._token
        cli._handler = self._handler
        cli._pollingTimeout = self._polling_timeout
        return cli
