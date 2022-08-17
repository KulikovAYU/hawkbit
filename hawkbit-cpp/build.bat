set SOL_DIR=%~dp0
set VCPKG_PATH=%~dp0..\vendors\vcpkg

mkdir build
cmake -B .\build -S . -DVCPKG_TARGET_TRIPLET=x64-windows -DCMAKE_TOOLCHAIN_FILE=%VCPKG_PATH%\scripts\buildsystems\vcpkg.cmake