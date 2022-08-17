#!/usr/bin/python3

import os
import sys
import pathlib
import subprocess
import shutil

# Some flags
is_windows = (sys.platform == 'win32') or (sys.platform == 'msys')
is_linux = sys.platform == 'linux'
is_osx = sys.platform == 'darwin'

deps = [
    'OpenSSL',
    # 'up2date-cpp'
    # 'hawkbit_interop',       # Also will grab libflac, libogg, and libvorbis for us
]
vcpkg_exe = ''
dlls = []
dll_renaming_rules = []  # list of two-element tuples
dll_src_dir = ''
dll_dest_dir = ''  # destination directory for dll
project_dir_path = os.getcwd()

# MSVS compiler data
ms_vs_platform_version = 142  # версия набора инструмента платформы к msVS
ms_vs_msbuild_path = "c:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe"  # путь к компилятору msVS
if is_windows and not os.path.exists(ms_vs_msbuild_path):
    print('Error, not able to find %s' % ms_vs_msbuild_path)
    sys.exit(1)

# project directory data
# main_project_dir = "/home/user/hawkbit_updater/4diac-rts-updater/ForteConfigurationLoader"  # рабочая директория проекта
main_project_dir = "J:\\RtSoft_practic\\4diac-rts-updater\\ForteConfigurationLoader"  # рабочая директория проекта
if is_windows and not os.path.exists(main_project_dir):
    print('Error, not able to find %s' % main_project_dir)
    sys.exit(1)

project_native_lib_path = os.path.join(main_project_dir, 'HawkbitNativeLibs')  # директория, где лежат собранные dll-ки
if not os.path.exists(project_native_lib_path):
    print('Error, not able to find %s' % project_native_lib_path)
    sys.exit(1)

# hawkbit source directory data
hawkbit_src_dir = os.path.join(main_project_dir,
                               'HawkbitNativeCppProj')  # директория, куда скачиваются исходники хакбита

# git credentials
user_name = 'akulikov'  # needs to fill
user_pwd = '1nwvq7qcv'  # needs to fill

if user_name == "":
    print('user_name must be defined')
    sys.exit(1)

if user_pwd == "":
    print('user_pwd must be defined')
    sys.exit(1)

# github data
hawkbit_git_repo_url = "https://" + user_name + ":" + user_pwd + "@" + "git.dev.rtsoft.ru/git/rtsedu/4diac-rts-updater.git"
hawkbit_git_repo_branch = "develop"

# Ensure we have access to the VCPKG executable, should be first argument
vcpkg_path = os.environ.get('VCPKG_DIR')
# if env variable doesn't set direct path
if not vcpkg_path:
    vcpkg_path = "J:\\RtSoft_practic\\hawkbit\\vendors\\vcpkg"
    # vcpkg_path = "/home/user/vcpkg/vcpkg"

if not os.path.exists(vcpkg_path):
    print('Error, not able to find %s' % vcpkg_path)
    sys.exit(1)

vcpkg_dir = os.path.abspath(vcpkg_path)
if not vcpkg_dir:
    print('Error, need environment variable VCPKG_DIR to point to directory where `vcpkg` executable is')
    sys.exit(1)
elif not os.path.exists(vcpkg_dir):
    print('Error, not able to find %s' % vcpkg_dir)
    sys.exit(1)


def check_return_code(ret_code_tag, message_action):
    if ret_code_tag == 0:
        print("\n" + message_action + " sucsess...")
    else:
        print("\n" + message_action + "failed ...")
        sys.exit(1)


def copy_files_to_directory(src_dir, dest_dir, extentions):
    src_files = os.listdir(src_dir)
    for file_name in src_files:
        full_file_name = os.path.join(src_dir, file_name)
        if os.path.isfile(full_file_name) and pathlib.Path(full_file_name).suffix in extentions:
            shutil.copy(full_file_name, dest_dir)


# clone git repository and update submodules
def clone_git_repo(git_adress, git_branch, download_local_folder):
    if os.path.exists(download_local_folder):
        return

    print("\nclonning hawkbit repo...")
    git_command = "git clone -b " + git_branch + " " + git_adress + " " + download_local_folder
    ret_code = subprocess.run(git_command, shell=True)
    check_return_code(ret_code.returncode, "clonning hawkbit repo")

    os.chdir(download_local_folder)
    print("\nupdating submodules...")
    ret_code = subprocess.run("git submodule update --recursive --remote --init", shell=True)
    check_return_code(ret_code.returncode, "updating submodules")

    os.chdir(project_dir_path)


# install vcpkg dependencies
def install_vcpkg_dependencies():
    subprocess.call([vcpkg_exe, 'install', *deps, '--overlay-triplets=dynamic-triplets'], shell=True)


# compile and build hawkbit client project
def build_hawkbit_client(download_local_folder):
    os.chdir(download_local_folder)
    subprocess.call(vcpkg_path + '/vcpkg install', shell=True)  # TODO: add json file

    build_dir_name = 'build'
    if os.path.exists(build_dir_name):
        shutil.rmtree(build_dir_name, ignore_errors=False, onerror=None)
    os.mkdir(build_dir_name)

    if is_windows:
        reply = subprocess.run(
            'cmake -T v' + str(
                ms_vs_platform_version) + ' -B build -S . -DCMAKE_TOOLCHAIN_FILE=' + vcpkg_path + '/scripts/buildsystems/vcpkg.cmake')
        check_return_code(reply.returncode, "run CMake build")
        os.chdir(build_dir_name)
        reply = subprocess.run(
            ms_vs_msbuild_path + ' ALL_BUILD.vcxproj /p:Platform=x64 /p:Configuration=Release /verbosity:quiet')
        check_return_code(reply.returncode, "run MSVS build")
        copy_files_to_directory(os.path.join(os.getcwd(), 'interop', 'Release'),
                                os.path.join(project_native_lib_path, 'Windows', 'Release_x64'), [".dll", ".lib", ".exp"])
    else:
        reply = subprocess.call(
            'cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=' + vcpkg_path + '/scripts/buildsystems/vcpkg.cmake',
            shell=True)
        check_return_code(reply, "run CMake build")
        os.chdir(build_dir_name)
        reply = subprocess.call('make', shell=True)
        check_return_code(reply, "run Build")
        copy_files_to_directory(os.path.join(os.getcwd(), 'interop'),
                                os.path.join(project_native_lib_path, 'Linux'), [".so"])

    os.chdir(project_dir_path)


# Some OS specific configurations
if is_windows:
    # If on windows, need to build the 64 bit version
    vcpkg_exe = os.path.join(vcpkg_dir, 'vcpkg.exe')
    # deps = ['%s:x64-windows' % x for x in deps]
    dll_src_dir = 'installed/x64-windows/bin/'
    dll_dest_dir = 'Windows/Release_x64'
    dlls = [
        'hawkbit_interop.dll',
        'libcrypto-1_1-x64.dll',
        'libssl-1_1-x64.dll',
    ]
    dll_renaming_rules = [

    ]
elif is_linux:
    vcpkg_exe = os.path.join(vcpkg_dir, 'vcpkg')
    dll_src_dir = 'installed/x64-linux/lib/'
    dll_dest_dir = 'Linux'
    dlls = [
        'libhawkbit_interop.so',
    ]
    dll_renaming_rules = [
        # ('libhawkbit_interop.so', 'hawkbit_interop.so')
    ]
elif is_osx:
    vcpkg_exe = os.path.join(vcpkg_dir, 'vcpkg')
    dll_src_dir = 'installed/x64-osx/lib/'
    dll_dest_dir = 'osx'
    dlls = [
        'libhawkbit_interop.dylib',
    ]
    dll_renaming_rules = [
        ('libhawkbit_interop.dylib', 'hawkbit_interop.dylib')
    ]

# First make sure the lib directory is there
pathlib.Path(project_native_lib_path).mkdir(exist_ok=True)

clone_git_repo(hawkbit_git_repo_url, hawkbit_git_repo_branch, hawkbit_src_dir)

install_vcpkg_dependencies()

build_hawkbit_client(hawkbit_src_dir)

# make full path directory
dll_full_path = os.path.join(project_dir_path, project_native_lib_path, dll_dest_dir)

# Now get the dlls that we really want
# for dll in dlls:
#    src = os.path.join(vcpkg_dir, dll_src_dir, dll)
#    shutil.copy(src, dll_full_path)

# Fix names according renaming rules
for (src_name, dst_name) in dll_renaming_rules:
    os.rename(
        os.path.join(dll_full_path, src_name),
        os.path.join(dll_full_path, dst_name)
    )

# if __name__ == "__main__":
#    //impl
