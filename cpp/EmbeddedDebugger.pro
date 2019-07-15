TEMPLATE    = subdirs
# Needed to ensure that things are built right, which you have to do yourself :(
CONFIG += ordered

SUBDIRS    = Connectors \
Profiles \
EmbeddedDebugger \

Profile.depends = Connectors
