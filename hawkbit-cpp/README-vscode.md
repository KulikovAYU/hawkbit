# vscode usage

see also https://code.visualstudio.com/docs/remote/remote-overview

SYSTEM REQUIREMENTS:
- docker
- vscode Remote Development extension pack

```shell   
git clone --recurse-submodules https://.../hawkbit-cpp.git
```

F1: Remote-Containers: Reopen in Container
F1: CMake: Configure
F1: CMake: Build

Run client:
```shell   
docker build . -t hawkbit_cpp:1.0   
docker run --rm -it -v "$(pwd)/client.conf:/opt/conf/client.conf" hawkbit_cpp:1.0   
```

If you have intermediate.crt and intermediate.key (OEM keypair) you can create certificate for device:
```shell   
./gen_certificate -c intermediate-ca.crt -k intermediate-ca.key -t my-tenant -n my-controller
```   
To format *client.conf* file use:
```shell
./format_configfile -c output/my-controller.crt -p "https://provisioning.pp" -x "my-token"
```
