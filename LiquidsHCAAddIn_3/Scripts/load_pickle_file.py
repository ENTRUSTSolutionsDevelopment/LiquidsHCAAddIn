from uuid import UUID
import sys
import os
from evoleap_licensing.InstanceIdentity import InstanceIdentity, InstanceIdentityValue
from evoleap_licensing.UserIdentity import UserIdentity
from evoleap_licensing.desktop.ControlManager import ControlManager
# from evoleap_licensing.webservice.ConnectionSettings import ConnectionSettings
from uuid import getnode as get_mac
import pickle


product_id = '9145E227-422B-434E-89A2-4EEB7AF106D4'
disable_str =  "Disable Updates"

rsa_public_key = "-----BEGIN PUBLIC KEY-----\n\
                           MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxVeHxDKgjjfiEKYRGbQ4\n\
                           xf6+JkRFjHi16Bc6jRs2Y8IWfp5KEuaMCqRF7w1tfjbUrfjpY0u2HTeDnJwETAmX\n\
                           vE3qzhnjaVlUKua60BgzKRNLOKx8DE55NX21U9N7jsnmRpe3gt/dJkMWiyANjPi5\n\
                           NdYR3NC3+tXHbO5gQiqobtns3+omGKxi4kfB8BEXGMQPnUui3M9VZMTi0lLLevUo\n\
                           v/jIUQ+x78jqUx5dFn38Nz1ErK7O/xsYdwSoQmDKjdlAhkzzHkgViR6JpJI9Yj5/\n\
                           NPxLh3fLiSx6UzAaa3MUNBO6CO/hMTePhWQsyY71D1uye+m7UsTG5xs1MQGnUJwQ\n\
                           oQIDAQAB\n\
                           -----END PUBLIC KEY-----"
# evoleap_server = 'https://elm.evoleap.com/'
version = "2.6.01"


def get_mac_address():
    """ read system mac address"""
    mac = get_mac()
    h = iter(hex(mac)[2:].zfill(12))
    return ":".join(i + next(h) for i in h)

def get_HDD_SN():
    return os.popen("wmic diskdrive get serialnumber").read().split()[-1]

def get_appdata_roaming_dir():
    try:
        appdata_dir = os.getenv('APPDATA')
        g2is_dir = os.path.join(appdata_dir, "G2-IS")
        liquids_dir = os.path.join(g2is_dir, "LiquidsHCA")
        if not os.path.exists(g2is_dir):
            os.makedirs(g2is_dir)
            os.makedirs(liquids_dir)
        else:
            if not os.path.exists(liquids_dir):
                os.makedirs(liquids_dir)
        return liquids_dir

    except:
        raise

def check_disable_updates():
    try:

        p_id = UUID(product_id)

        instance_identity_info = {'mac': InstanceIdentityValue(get_mac_address()),
                                  'hddsn': InstanceIdentityValue(get_HDD_SN())}
        user_identity_info = {'UPN': os.getlogin()}
        # ConnectionSettings.SetHost(evoleap_server)
        user_identity = UserIdentity(user_identity_info)
        instance_identity = InstanceIdentity(instance_identity_info)
        in_dir = get_appdata_roaming_dir()
        file = os.path.join(in_dir, product_id + ".pkl")
        if not os.path.exists(file):
            return False
        with open(file, 'rb') as input:
            vs = pickle.load(input)

        manager = ControlManager(p_id, version, rsa_public_key, user_identity, instance_identity,saved_state=vs)
        if len(manager.State.Features) > 0 and disable_str in manager.State.Features:
            return True
        else:
            return False


    except Exception as e:
        return False


disable_updates = check_disable_updates()
print(disable_updates)