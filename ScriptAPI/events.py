from utils import log
from Void.Scripting import ScriptAPI

class Responder():

    def get_list(self, msgid):
        l = self.dict.get(msgid)
        if l is None: 
            # log('constructing list for event ' + str(msgid))
            self.dict[msgid] = l = []
        return self.dict[msgid]

    def __init__(self, id):

        def _handle_message(msgid, payload):
            l = self.get_list(msgid)
            for item in self.get_list(msgid): item(payload)
            
        self.id = id
        self.pump = ScriptAPI.GetMessagePump(id)
        # log('responder will be responding to ' + id + ' messages')
        self.pump.OnMessage += _handle_message
        hc = self.pump.GetHashCode().ToString('x')
        # log('hashcode = ' + str(hc))
        self.dict = {}

    def register(self, msgid, func):
        # log('registering ' + self.id + ':' + msgid + ' : ' + str(func))
        self.get_list(msgid).append(func)

pump_responders = { }

def _get_or_create_responder(id):
    r = pump_responders.get(id);
    if r is None: r = pump_responders[id] = Responder(id)
    return r
    
def respond(pump_id, event_id):
    def response_wrapper(func):
        _get_or_create_responder(pump_id).register(event_id, func)
        pass

    return response_wrapper