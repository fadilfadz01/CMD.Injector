#########################################################################################
#
# Copyright (c) Microsoft Corporation. All rights reserved.
#
# Localized PnpDevice.Resource.psd1
#
#########################################################################################

ConvertFrom-StringData @'
###PSLOC
    IDS_PROB_NOT_CONFIGURED=This device is not configured correctly. {0}
    IDS_PROB_DEVLOADERFAILED=Windows cannot load the driver for this device. {0}
    IDS_PROB_OUT_OF_MEMORY=The driver for this device might be corrupted, or your system may be running low on memory or other resources. {0}
    IDS_PROB_WRONG_TYPE=This device is not working properly. One of its drivers or your registry might be corrupted. {0}
    IDS_PROB_LACKEDARBITRATOR=The driver for this device needs a resource that Windows cannot manage. {0}
    IDS_PROB_BOOT_CONFIG_CONFLICT=The boot configuration for this device conflicts with other devices. {0}
    IDS_PROB_FAILED_FILTER=Cannot filter. {0}
    IDS_PROB_DEVLOADER_NOT_FOUND=The driver loader for the device is missing. {0}
    IDS_PROB_INVALID_DATA=Windows cannot identify this hardware because it does not have a valid hardware identification number. {0}\r\n\r\nFor assistance, contact the hardware manufacturer.
    IDS_PROB_FAILED_START=This device cannot start. {0}
    IDS_PROB_LIAR=This device failed. {0}
    IDS_PROB_NORMAL_CONFLICT=This device cannot find enough free resources that it can use. {0}\r\n\r\nIf you want to use this device, you will need to disable one of the other devices on this system.
    IDS_PROB_NOT_VERIFIED=Windows cannot verify this device's resources. {0}
    IDS_PROB_NEEDRESTART=This device cannot work properly until you restart your computer. {0}
    IDS_PROB_REENUMERATION=This device is not working properly because there is probably a reenumeration problem. {0}
    IDS_PROB_PARTIALCONFIG=Windows cannot identify all the resources this device uses. {0}
    IDS_PROB_UNKNOWN_RESOURCE=This device is asking for an unknown resource type. {0}
    IDS_PROB_REINSTALL=Reinstall the drivers for this device. {0}
    IDS_PROB_REGISTRY=Windows cannot start this hardware device because its configuration information (in the registry) is incomplete or damaged. {0}
    IDS_PROB_WILL_BE_REMOVED=Windows is uninstalling this device. {0}
    IDS_PROB_DISABLED=This device is disabled. {0}
    IDS_DEVICE_NOT_THERE=This device is not present, is not working properly, or does not have all its drivers installed. {0}
    IDS_PROB_MOVED=Windows is still setting up this device. {0}
    IDS_PROB_TOO_EARLY=Windows is still setting up this device. {0}
    IDS_PROB_NO_VALID_LOG_CONF=This device does not have valid log configuration. {0}
    IDS_PROB_FAILEDINSTALL=The drivers for this device are not installed. {0}
    IDS_PROB_HARDWAREDISABLED=This device is disabled because the firmware of the device did not give it the required resources. {0}
    IDS_PROB_CANT_SHARE_IRQ=This device is using an Interrupt Request (IRQ) resource that another device is using. {0}
    IDS_PROB_FAILED_ADD=This device is not working properly because Windows cannot load the drivers required for this device. {0}
    IDS_PROB_DISABLED_SERVICE=A driver (service) for this device has been disabled.  An alternate driver may be providing this functionality. {0}
    IDS_PROB_TRANSLATION_FAILED=Windows cannot determine which resources are required for this device. {0}
    IDS_PROB_NO_SOFTCONFIG=Windows cannot determine the settings for this device.  Consult the documentation that came with this device and use the Resource tab to set the configuration. {0}
    IDS_PROB_BIOS_TABLE=Your computer's system firmware does not include enough information to properly configure and use this device.  To use this device, contact your computer manufacturer to obtain a firmware or BIOS update. {0}
    IDS_PROB_IRQ_TRANSLATION_FAILED=This device is requesting a PCI interrupt but is configured for an ISA interrupt (or vice versa).  Please use the computer's system setup program to reconfigure the interrupt for this device. {0}
    IDS_PROB_FAILED_DRIVER_ENTRY=Windows cannot initialize the device driver for this hardware. {0}
    IDS_PROB_DRIVER_FAILED_PRIOR_UNLOAD=Windows cannot load the device driver for this hardware because a previous instance of the device driver is still in memory. {0}
    IDS_PROB_DRIVER_FAILED_LOAD=Windows cannot load the device driver for this hardware. The driver may be corrupted or missing. {0}
    IDS_PROB_DRIVER_SERVICE_KEY_INVALID=Windows cannot access this hardware because its service key information in the registry is missing or recorded incorrectly. {0}
    IDS_PROB_LEGACY_SERVICE_NO_DEVICES=Windows successfully loaded the device driver for this hardware but cannot find the hardware device. {0}
    IDS_PROB_DUPLICATE_DEVICE=Windows cannot load the device driver for this hardware because there is a duplicate device already running in the system. {0}
    IDS_PROB_FAILED_POST_START=Windows has stopped this device because it has reported problems. {0}
    IDS_PROB_HALTED=An application or service has shut down this hardware device. {0}
    IDS_PROB_PHANTOM=Currently, this hardware device is not connected to the computer. {0}\r\n\r\nTo fix this problem, reconnect this hardware device to the computer.
    IDS_PROB_SYSTEM_SHUTDOWN=Windows cannot gain access to this hardware device because the operating system is in the process of shutting down. {0}\r\n\r\nThe hardware device should work correctly next time you start your computer.
    IDS_PROB_HELD_FOR_EJECT=Windows cannot use this hardware device because it has been prepared for ""safe removal"", but it has not been removed from the computer. {0}\r\n\r\nTo fix this problem, unplug this device from your computer and then plug it in again.
    IDS_PROB_DRIVER_BLOCKED=The software for this device has been blocked from starting because it is known to have problems with Windows. Contact the hardware vendor for a new driver. {0}
    IDS_PROB_REGISTRY_TOO_LARGE=Windows cannot start new hardware devices because the system hive is too large (exceeds the Registry Size Limit). {0}\r\n\r\nTo fix this problem, you should first try uninstalling any hardware devices that you are no longer using. If that doesn't solve the problem, then you will have to reinstall Windows.
    IDS_PROB_SETPROPERTIES_FAILED=Windows cannot apply all of the properties for this device. Device properties may include information that describes the device's capabilities and settings (such as security settings for example).  {0}\r\n\r\nTo fix this problem, you can try reinstalling this device. However, it is recommended that you contact the hardware manufacturer for a new driver.
    IDS_PROB_SYSTEMFAILURE=System failure: Try changing the driver for this device. If that doesn't work, see your hardware documentation. {0}
    IDS_PROB_NOPROBLEM=This device is working properly.
    IDS_PROB_UNKNOWN=This device has a problem, but Windows cannot determine what the problem is.
    IDS_PROB_UNKNOWN_WITHCODE=This device has a problem that Windows cannot identify. {0}
    IDS_PROB_CODE=(Code {0}).
    IDS_PROB_WAITING_ON_DEPENDENCY=This device is currently waiting on another device or set of devices to start. {0}
    IDS_PROB_UNSIGNED_DRIVER=Windows cannot verify the digital signature for the drivers required for this device. A recent hardware or software change might have installed a file that is signed incorrectly or damaged, or that might be malicious software from an unknown source. {0}
    IDS_PROB_USED_BY_DEBUGGER=This device has been reserved for use by the Windows kernel debugger for the duration of this boot session. {0}
    IDS_PROB_DEVICE_RESET=This device has failed and is undergoing a reset. {0}
###PSLOCC
'@