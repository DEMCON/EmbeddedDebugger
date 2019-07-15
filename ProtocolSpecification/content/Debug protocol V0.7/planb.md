

Debug protocol splitted into capabilities

# Pre-defined capabilities

## Capability querying

Each capability has a unique name.

name: "capability.query"

## Trace capability

name: "debug.signal.trace"

Functionality:
- Get available signals
- get number of signals
- Get signal name -> get name of a signal
- Configure channel -> configure channel to specific signal
- Set decimation -> ?
- Get data -> Fetch collected data.

### API

FUNCTION {
    name: Get trace signals
    input: []
    result: [list of strings]
}

FUNCTION {
    name: get data
    input: []
    result: [list of data]
}
