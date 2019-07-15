TEMPLATE        = lib
CONFIG         += plugin
CONFIG += staticlib
QT              += network widgets
HEADERS         = TCP.h \
    ../DebugProtocolV0/ApplicationLayerV0.h \
    ../DebugProtocolV0/DebugProtocolV0Enums.h \
    ../DebugProtocolV0/PresentationLayerV0.h \
    ../DebugProtocolV0/TransportLayerV0.h \
    ../BaseInterface/ApplicationLayerBase.h \
    ../BaseInterface/PresentationLayerBase.h \
    ../BaseInterface/TransportLayerBase.h \
    ../../EmbeddedDebugger/Medium/Register/Register.h \
    ../../EmbeddedDebugger/Medium/Register/RegisterListModel.h \
    ../../EmbeddedDebugger/Medium/CPU/Cpu.h \
    ../../EmbeddedDebugger/Medium/CPU/CpuListModel.h \
    ../../EmbeddedDebugger/Medium/Medium.h \
    ../BaseInterface/Common.h \
    ../../Profiles/kconcatenaterowsproxymodel.h \
    Settings.h \
    Settings.h

SOURCES         = TCP.cpp \
    ../DebugProtocolV0/ApplicationLayerV0.cpp \
    ../DebugProtocolV0/PresentationLayerV0.cpp \
    ../DebugProtocolV0/TransportLayerV0.cpp \
    ../../EmbeddedDebugger/Medium/Register/Register.cpp \
    ../../EmbeddedDebugger/Medium/Register/RegisterListModel.cpp \
    ../../EmbeddedDebugger/Medium/CPU/Cpu.cpp \
    ../../EmbeddedDebugger/Medium/CPU/CpuListModel.cpp \
    ../../Profiles/kconcatenaterowsproxymodel.cpp \
    Settings.cpp \
    Settings.cpp

TARGET          = $$qtLibraryTarget(Tcp)
DESTDIR         = ../../plugins
INCLUDEPATH += ../../EmbeddedDebugger/

FORMS += \
    Settings.ui \
    Settings.ui

