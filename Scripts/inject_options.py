from utils import log
import events

@respond("ui", "construct_options")
def inject_options(payload):
    log('inject_options called')
    pass
