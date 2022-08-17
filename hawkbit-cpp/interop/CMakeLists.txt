set(HAWKBIT_INTEROP_PROJECT_NAME hawkbit_interop)
project(${HAWKBIT_INTEROP_PROJECT_NAME} LANGUAGES CXX)

include_directories(headers)

set(HEADER_FILES headers/hawkbitApi.hpp
		headers/hawkbitPrototypes.hpp)

set(SRC_FILES src/main.cpp )

if (UNIX)
    SET (CMAKE_CXX_FLAGS "-fPIC" CACHE STRING "compile flags" FORCE)
endif (UNIX)

add_library(${PROJECT_NAME} SHARED ${HEADER_FILES} ${SRC_FILES})

set_target_properties(${PROJECT_NAME} PROPERTIES LINKER_LANGUAGE CXX)

target_link_libraries(${PROJECT_NAME} sub::hawkbitl)

target_compile_features(${PROJECT_NAME} PUBLIC cxx_std_17)