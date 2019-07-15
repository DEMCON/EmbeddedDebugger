TEMPLATE        = lib
CONFIG         += plugin
QT              += network widgets
TARGET          = $$qtLibraryTarget(GenericTcpProfile)
DESTDIR         = ../../plugins

LIBS += -L../../plugins -lTcpd


HEADERS += \
    GenericTcpProfile.h \
    ../BaseProfile.h \
    ../kconcatenaterowsproxymodel.h

SOURCES += \
    GenericTcpProfile.cpp \
    ../kconcatenaterowsproxymodel.cpp

INCLUDEPATH += ../../EmbeddedDebugger/
