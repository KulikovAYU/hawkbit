from enum import Enum


class ResponseDeliveryListener:
    def on_successful_delivery(self):
        return

    def on_error(self):
        return


class Response:
    class Finished(Enum):
        NONE = 0
        SUCCESS = 1
        FAILURE = 2

    class Execution(Enum):
        NONE = 0
        CLOSED = 1
        PROCEEDING = 2
        CANCELED = 3
        SCHEDULED = 4
        REJECTED = 5
        RESUMED = 6

    def __init__(self):
        self._finished = self.Finished.NONE
        self._execution = self.Execution.NONE
        self._details = []
        self._response_delivery_listener = None
        self._ignore_sleep = False

    def set_finished(self, finished: Finished):
        self._finished = finished
        return self

    def set_execution(self, execution: Execution):
        self._execution = execution
        return self

    def add_detail(self, detail):
        self._details.append(detail)
        return self

    def set_response_delivery_listener(self, response_delivery_listener):
        self._response_delivery_listener = response_delivery_listener
        return self

    def set_ignore_sleep(self):
        self._ignore_sleep = True
        return self

    @property
    def finished(self):
        return self._finished

    @property
    def execution(self):
        return self._execution

    @property
    def response_delivery_listener(self):
        return self._response_delivery_listener

    @property
    def ignore_sleep(self):
        return self._ignore_sleep

    # def get_finished(self): return self._finished

    # def get_execution(self): return self._execution


class ConfigResponse:
    def __init__(self):
        self._data = {}
        self._ignore_sleep = False

    def add_data(self, key, value):
        self._data[key] = value
        return self

    def set_ignore_sleep(self):
        self._ignore_sleep = True
        return self

    def is_ignore_sleep(self) -> bool:
        return self._ignore_sleep

    def get_data(self) -> {}:
        return self._data
