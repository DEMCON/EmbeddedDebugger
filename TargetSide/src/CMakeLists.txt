
# Enable the usage of the embedded debugger by copy-pasting this folder into
# an embedded project.

add_library(
    embeddeddebugger
    debugChannel.c
    debugMessage.c
    debugProtocol.c
)

