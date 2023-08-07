from Void import GeneralTester

def call_static_method_to_return_foo():
    return GeneralTester.ReturnFoo()

def return_bar_from_constructed_instance():
    t = GeneralTester()
    return t.ReturnBar()

def return_bar_from_provided_instance(inst):
    return inst.ReturnBar()