#pragma once

namespace hawkbit {
    const int HTTP_UNAUTHORIZED = 401;
    const int HTTP_OK = 200;
    const int HTTP_CREATED = 201;

    const std::string UNAUTHORIZED_ERROR_MESSAGE = "Got " + std::to_string(HTTP_UNAUTHORIZED) + " code";

    // throws if returned unauthorized HTTP code from hawkBit
    class unauthorized_exception : public std::exception {
    public:
        const char *what() const noexcept override {
            return UNAUTHORIZED_ERROR_MESSAGE.c_str();
        }
    };

    // throws if returned unexpected HTTP code
    class http_unexpected_code_exception : public std::exception {
        std::string message;

    public:
        http_unexpected_code_exception(int presented, int expected) {
            message = "Unexpected code. Got " + std::to_string(presented) + " (expected " + std::to_string(expected) +
                      ")";
        }

        const char *what() const noexcept override {
            return message.c_str();
        }
    };

    // throws if payload from hawkbit is invalid
    class unexpected_payload : public std::exception {
    public:
        const char *what() const noexcept override {
            return "unexpected payload";
        }
    };

    // throws if got unexpected data from client-defined handler
    class wrong_response : public std::exception {
    public:
        const char *what() const noexcept override {
            return "wrong response";
        }
    };

    class http_lib_error: public std::exception {
        std::string message;
    public:
        http_lib_error(int error_num) {
            message = "HTTP request error. Error code " + std::to_string(error_num);
        }
        const char *what() const noexcept override {
            return message.c_str();
        }
    };

    class authority_type_exception: public std::exception {
        std::string message;
    public:
        authority_type_exception(const std::string &message_) {
            message = message_;
        }
        const char *what() const noexcept override {
            return message.c_str();
        }
    };
}