import json


class Hashes:
    def __init__(self):
        self._sha1 = ""
        self._md5 = ""
        self._sha256 = ""


class Artifact:
    def __init__(self):
        self._file_name = ""
        self._file_hashes: Hashes
        self._file_size = 0
        self._download_uri = ""
        self._download_provider = None

    def download_to(self, path):
        if self._download_provider is not None:
            self._download_provider.download_to(self._download_uri, path)


class Chunk:
    def __init__(self):
        self._part = ""
        self._version = ""
        self._name = ""
        self._artifacts: Artifact = []

    @property
    def artifacts(self):
        return self._artifacts


class DeploymentBase:

    def __init__(self):
        self._id = -1
        self._download_type = ""
        self._update_type = ""
        self._in_maintenance_window = False
        self._chunks: Chunk = []

    @property
    def id(self):
        return self._id

    @property
    def download_type(self):
        return self._download_type

    @property
    def update_type(self):
        return self._update_type

    @property
    def in_maintenance_window(self):
        return self._in_maintenance_window

    @property
    def chunks(self):
        return self._chunks

    @staticmethod
    def from_body(body, request_formatter):
        document = json.loads(body.text)

        if "id" not in document or \
                "deployment" not in document or \
                "update" not in document["deployment"] or \
                "download" not in document["deployment"] or \
                "chunks" not in document["deployment"]:
            raise ValueError("invalid deployment JSON structure")

        return_depl = DeploymentBase()
        return_depl._id = document["id"]
        return_depl._update_type = document["deployment"]["update"]
        return_depl._download_type = document["deployment"]["download"]

        if "maintenanceWindow" in document["deployment"]:
            window = document["deployment"]["maintenanceWindow"]
            return_depl._in_maintenance_window = (window != "unavailable")
        else:
            return_depl._in_maintenance_window = True

        chunks = document["deployment"]["chunks"]
        for chunk in chunks:
            if "part" not in chunk or \
                    "version" not in chunk or \
                    "name" not in chunk or \
                    "artifacts" not in chunk:
                raise ValueError("invalid JSON payload")

            new_chunk = Chunk()
            new_chunk._part = chunk["part"]
            new_chunk._name = chunk["name"]
            new_chunk._version = chunk["version"]

            artifacts = chunk["artifacts"]
            for artifact in artifacts:
                if "filename" not in artifact or \
                        "hashes" not in artifact or \
                        "size" not in artifact or \
                        "_links" not in artifact or \
                        "sha256" not in artifact["hashes"] or \
                        "sha1" not in artifact["hashes"] or \
                        "md5" not in artifact["hashes"] or \
                        "download-http" not in artifact["_links"]:
                    raise ValueError("invalid JSON artifact")

                hash_data = Hashes()
                hash_data._sha1 = artifact["hashes"]["sha1"]
                hash_data._md5 = artifact["hashes"]["md5"]
                hash_data._sha256 = artifact["hashes"]["sha256"]

                artifact_data = Artifact()
                artifact_data._file_name = artifact["filename"]
                artifact_data._file_size = artifact["size"]
                artifact_data._download_uri = artifact["_links"]["download-http"]["href"]
                artifact_data._file_hashes = hash_data
                artifact_data._download_provider = request_formatter
                new_chunk.artifacts.append(artifact_data)

        return_depl._chunks.append(new_chunk)
        return return_depl


class CancelAction:
    def __init__(self):
        self._id = 0
        self._stop_id = 0

    @property
    def id(self):
        return self._id

    @property
    def stop_id(self):
        return self._stop_id

    @staticmethod
    def from_string(body):
        document = json.loads(body.text)
        if "id" not in document or \
                "cancelAction" not in document or \
                "stopId" not in document["cancelAction"]:
            raise ValueError("invalid cancel action JSON structure")

        cancel_action = CancelAction()
        cancel_action._stop_id = int(document["cancelAction"]["stopId"])
        cancel_action._id = int(document["id"])
        return cancel_action
