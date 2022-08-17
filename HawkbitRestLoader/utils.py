from Hawkbit.hawkbit_response import Response


class Utils:
    @staticmethod
    def finished_to_string(finished):
        if finished == Response.Finished.SUCCESS:
            return "success"
        elif finished == Response.Finished.FAILURE:
            return "failure"
        elif finished == Response.Finished.NONE:
            return "none"

        raise ValueError("invalid finished status")

    @staticmethod
    def execution_to_string(execution):
        if execution == Response.Execution.CLOSED:
            return "closed"
        elif execution == Response.Execution.PROCEEDING:
            return "proceeding"
        elif execution == Response.Execution.CANCELED:
            return "canceled"
        elif execution == Response.Execution.SCHEDULED:
            return "scheduled"
        elif execution == Response.Execution.REJECTED:
            return "rejected"
        elif execution == Response.Execution.RESUMED:
            return "resumed"

        raise ValueError("invalid execution status")
