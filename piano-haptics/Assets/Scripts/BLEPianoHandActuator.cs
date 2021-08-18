using System;
using System.Collections;
using UnityEngine;

#if WINDOWS_UWP
using System.Linq;
using Windows.Devices.Enumeration;
using System.Collections.ObjectModel;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif

public class BLEPianoHandActuator : MonoBehaviour
{
    #region Error Codes
    readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
    readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
    readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
    readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
    #endregion

#if WINDOWS_UWP
    private DeviceWatcher deviceWatcher = null;
    private BluetoothLEDevice bleActuatorDevice = null;
    private GattDeviceService bleService = null;
#endif

    public string bluetoothDeviceName;
    public string serviceUuid;



    // Start is called before the first frame update
  /*  void Start()
    {
        CreateAndStartBleDeviceWatcher();
        InvokeRepeating("RestartLookingForActuatorIfNecessary", 35, 35);
    }*/



    private void RestartLookingForActuatorIfNecessary()
    {
#if WINDOWS_UWP

       
            bool isConnectedToActuator =IsConnectedToActuator();
            Debug.Log("RestartLookingForActuatorIfNecessary isConnectedToActuator="+ isConnectedToActuator);
            if (!isConnectedToActuator)
            {
                RestartDeviceWatcher();
            }
       
#endif

    }

    private void OnDestroy()
    {
        StopBleDeviceWatcher();
    }

    void CreateAndStartBleDeviceWatcher()
    {
#if WINDOWS_UWP

        Debug.Log("CreateAndStartBleDeviceWatcher");
        string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

        deviceWatcher = DeviceInformation.CreateWatcher(
            BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint
        );

        // Register event handlers before starting the watcher.
        deviceWatcher.Added += DeviceWatcher_Added;
        deviceWatcher.Updated += DeviceWatcher_Updated;
        deviceWatcher.Removed += DeviceWatcher_Removed;
        deviceWatcher.Stopped += DeviceWatcher_Stopped;

        // Start over with an empty collection.
        DisconnectActuator();

        // Start the watcher. Active enumeration is limited to approximately 30 seconds.
        // This limits power usage and reduces interference with other Bluetooth activities.
        // To monitor for the presence of Bluetooth LE devices for an extended period,
        // use the BluetoothLEAdvertisementWatcher runtime class. See the BluetoothAdvertisement
        // sample for an example.
        deviceWatcher.Start();
#endif

    }
#if WINDOWS_UWP


    private void DisconnectActuator()
    {
        lock (this)
        {
            DisposeActuator(this.bleActuatorDevice, this.bleService);
            bleActuatorDevice = null;
            bleService = null;
        }
    }

    private void DisposeActuator(BluetoothLEDevice bleActuatorDevice, GattDeviceService bleService)
    {

        if (bleActuatorDevice != null)
        {
            bleActuatorDevice.Dispose();
            Debug.Log("Disposed bleActuatorDevice");

        }
        if (bleService != null)
        {
            bleService.Dispose();

        }


    }



    private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
    {
       if (deviceInfo.Name == bluetoothDeviceName)
        {
            
            var actuatorToConnectTo = await GetActuatorToConnect(deviceInfo.Id);
            var serviceForFingers = await GetServiceForFingers(actuatorToConnectTo);

            bool isNextActuatorConnected = IsConnectedToActuator(actuatorToConnectTo, serviceForFingers);
            Debug.Log(string.Format("... Adding Id='{0}',Name='{1}',NewIsConnected='{2}'", deviceInfo.Id, deviceInfo.Name, isNextActuatorConnected));
            if (isNextActuatorConnected)
            {
                lock (this)
                {
                Debug.Log("Locked received");
                    if (sender == deviceWatcher)
                    {
                    if(actuatorToConnectTo == null)
                            {
                                Debug.Log("Could not connect");
                            }
                            if(serviceForFingers == null)
                            {
                                Debug.Log("Could not find service");
                            }
                            bool isAnotherActuatorConnected = IsConnectedToActuator();
                        Debug.Log(string.Format("Is currently a different actuator connected='{0}'", isAnotherActuatorConnected));

                        if (!isAnotherActuatorConnected)
                        {
                            DisconnectActuator();
                            this.bleActuatorDevice = actuatorToConnectTo;
                            this.bleService = serviceForFingers;

                            Debug.Log(string.Format("Added Id='{0}',Name='{1}'", deviceInfo.Id, deviceInfo.Name));
                            return;
                        }
                    }
                }
            }
            DisposeActuator(actuatorToConnectTo, serviceForFingers);

        }
    }

    private async Task<BluetoothLEDevice> GetActuatorToConnect(string deviceId)
    {

        try
        {
            // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
            return await BluetoothLEDevice.FromIdAsync(deviceId);


        }
        catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
        {
            Debug.Log("Bluetooth radio is not on.");
        }
        return null;
    }

    private async Task<GattDeviceService> GetServiceForFingers(BluetoothLEDevice deviceToGetServiceFrom)
    {
        if (deviceToGetServiceFrom != null)
        {
            var result = await deviceToGetServiceFrom.GetGattServicesForUuidAsync(new Guid(serviceUuid));
            if (result != null && result.Status == GattCommunicationStatus.Success)
            {
                return result.Services.FirstOrDefault();

            }
        }
        return null;
    }



    private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
    {
    
                       
   
        var actuatorToConnectTo = await GetActuatorToConnect(deviceInfoUpdate.Id);
        var serviceForFingers = await GetServiceForFingers(actuatorToConnectTo);
        if (IsConnectedToActuator(actuatorToConnectTo, serviceForFingers))
        {
            lock (this)
            {
                if (!IsConnectedToActuator() && sender == deviceWatcher && actuatorToConnectTo.DeviceInformation.Name == bluetoothDeviceName)
                {
                     Debug.Log(string.Format("Updated Id='{0}'", deviceInfoUpdate.Id));
                    
                        DisconnectActuator();
                        this.bleActuatorDevice = actuatorToConnectTo;
                        this.bleService = serviceForFingers;

                        Debug.Log(string.Format("Updated Id='{0}',Name='{1}'", bleActuatorDevice.DeviceId, bleActuatorDevice.DeviceInformation.Name));
                        return;
                    
                }
            }
        }
        DisposeActuator(actuatorToConnectTo, serviceForFingers);
       
    }

    private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
    {
       /* lock (this)
        {


            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == deviceWatcher && bleActuatorDevice != null && bleActuatorDevice.DeviceInformation.Id == deviceInfoUpdate.Id)
            {
                DisconnectActuator();
                Debug.Log(string.Format("Removed Id='{0}'", deviceInfoUpdate.Id));

            }
        }*/
    }


    private void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
    {
        lock (this)
        {
            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == deviceWatcher && !IsConnectedToActuator())
            {
                RestartDeviceWatcher();
                Debug.Log("DeviceWatcher_Stopped");
            }
        }
    }

    private bool IsConnectedToActuator(BluetoothLEDevice bleActuatorDevice, GattDeviceService bleService)
    {
        return bleActuatorDevice != null && bleActuatorDevice.ConnectionStatus == BluetoothConnectionStatus.Connected && bleService != null;
    }

    private void RestartDeviceWatcher()
    {
        lock (this)
        {

            if (!IsConnectedToActuator())
            {
                if (deviceWatcher == null)
                    {
                        Debug.Log("RestartDeviceWatcher deviceWatcher == null");
                        CreateAndStartBleDeviceWatcher();
                    }
                    else if (deviceWatcher.Status == DeviceWatcherStatus.Stopped || deviceWatcher.Status == DeviceWatcherStatus.Aborted || deviceWatcher.Status == DeviceWatcherStatus.Created)
                    {
                        Debug.Log("RestartDeviceWatcher");
                        deviceWatcher.Start();
                    }
            }

        }
    }
#endif

    public bool IsConnectedToActuator()
    {
#if WINDOWS_UWP
        return IsConnectedToActuator(this.bleActuatorDevice, this.bleService);
#endif
        return false;

    }

    /// <summary>
    /// Stops watching for all nearby Bluetooth devices.
    /// </summary>
    private void StopBleDeviceWatcher()
    {
#if WINDOWS_UWP

        if (deviceWatcher != null)
        {
            // Unregister the event handlers.
            deviceWatcher.Added -= DeviceWatcher_Added;
            deviceWatcher.Updated -= DeviceWatcher_Updated;
            deviceWatcher.Removed -= DeviceWatcher_Removed;
            deviceWatcher.Stopped -= DeviceWatcher_Stopped;

            // Stop the watcher.
            deviceWatcher.Stop();
            deviceWatcher = null;

            Debug.Log("StopBleDeviceWatcher");

        }
#endif

    }

    public async void Write(Guid characteristicToWriteToId, int value)
    {

#if WINDOWS_UWP
       // Debug.Log($"write bleServive={bleService}");
        if (IsConnectedToActuator())
        {
            var resultCharacteristics = await bleService.GetCharacteristicsForUuidAsync(characteristicToWriteToId);
            if (resultCharacteristics == null)
            {
                return;
            }
            GattCharacteristic characteristic = resultCharacteristics.Characteristics.FirstOrDefault();
            if (characteristic != null && characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
            {
                var writer = new DataWriter();
                writer.ByteOrder = ByteOrder.LittleEndian;
                writer.WriteInt32(value);


                try
                {
                    // BT_Code: Writes the value from the buffer to the characteristic.
                    var result = await characteristic.WriteValueWithResultAsync(writer.DetachBuffer());

                    if (result.Status != GattCommunicationStatus.Success)
                    {
                        Debug.Log($"Write failed: {result.Status}");
                    }
                }
                catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_INVALID_PDU)
                {
                    Debug.Log(ex.Message);
                    return;
                }
                catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED || ex.HResult == E_ACCESSDENIED)
                {
                    // This usually happens when a device reports that it support writing, but it actually doesn't.
                    Debug.Log(ex.Message);
                    return;
                }
            }
        }
#endif


    }



    /*
#if WINDOWS_UWP
    private DeviceWatcher deviceWatcher = null;
    private BluetoothLEDevice bleActuatorDevice = null;
    private GattDeviceService bleService = null;


#endif
    // Start is called before the first frame update
    void Start()
    {
        StartBleDeviceWatcher();
    }

    void StartBleDeviceWatcher()
    {
#if WINDOWS_UWP
        string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

        deviceWatcher = DeviceInformation.CreateWatcher(
            BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint
        );

        // Register event handlers before starting the watcher.
        deviceWatcher.Added += DeviceWatcher_Added;
        deviceWatcher.Updated += DeviceWatcher_Updated;
        deviceWatcher.Removed += DeviceWatcher_Removed;
        deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
        deviceWatcher.Stopped += DeviceWatcher_Stopped;

        // Start over with an empty collection.
        DisconnectActuator();

        // Start the watcher. Active enumeration is limited to approximately 30 seconds.
        // This limits power usage and reduces interference with other Bluetooth activities.
        // To monitor for the presence of Bluetooth LE devices for an extended period,
        // use the BluetoothLEAdvertisementWatcher runtime class. See the BluetoothAdvertisement
        // sample for an example.
        deviceWatcher.Start();
#endif
    }

    private void DisconnectActuator()
    {
#if WINDOWS_UWP
        if(bleActuatorDevice != null)
        {
                bleActuatorDevice.Dispose();
                bleActuatorDevice = null;

        }
        if (bleService != null)
        {
            bleService.Dispose();
            bleService = null;
        }
#endif
    }

    private void Update()
    {
        reconnectIfNecessary();
    }

    private void OnDestroy()
    {
#if WINDOWS_UWP
        if (watcher != null)
        {
            watcher.Received -= OnAdvertisementReceived;
            watcher.Stop();
            watcher = null;
        }
        if (bleActuatorDevice != null)
        {
            bleActuatorDevice.Dispose();
            bleActuatorDevice = null;
        }
        if (bleService != null)
        {
            bleService = null;
        }
#endif
    }

#if WINDOWS_UWP
    private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
    {

        // The local name of the advertising device contained within the payload, if any
        string localName = eventArgs.Advertisement.LocalName;

        Debug.Log(string.Format("OnAdvertisementReceived {0} {1}", eventArgs.BluetoothAddress.ToString(), localName));


        if (localName == bluetoothDeviceName)
        {
            lock (this)
            {

                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == watcher)
                {
                    connectToActuator(eventArgs.BluetoothAddress);

                }
            }
        }
    }
    private async void connectToActuator(ulong actuatorBluetoothAddress)
    {
        if (bleActuatorDevice != null && bleActuatorDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
        {
            return;
        }
        try
        {

            // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
            bleActuatorDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(actuatorBluetoothAddress);
            if (bleActuatorDevice == null)
            {
                Debug.Log("Failed to connect to device.");
            }
            else
            {
                removeService();
            }

        }
        catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
        {
            bleActuatorDevice = null;

            Debug.Log("Bluetooth radio is not on.");
            return;
        }
        findService();
    }

    private void removeService()
    {
        if (bleService != null)
        {
            bleService.Dispose();
            bleService = null;
        }
    }

    private void removeDevice()
    {
        if (bleActuatorDevice != null && bleActuatorDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
        {
            bleActuatorDevice.Dispose();
            bleActuatorDevice = null;
        }
    }

    private async void findService()
    {
        if (bleActuatorDevice != null && bleService == null)
        {
            var services = await bleActuatorDevice.GetGattServicesForUuidAsync(new Guid(serviceUuid));
            if (services != null && services.Status == GattCommunicationStatus.Success)
            {
                bleService = services.Services.FirstOrDefault();
                if (bleService == null)
                {
                    removeDevice();


                }
                else
                {
                    watcher.Stop();
                }
            }


        }
    }
#endif

    private void reconnectIfNecessary()
    {
#if WINDOWS_UWP
        if (bleActuatorDevice == null || bleActuatorDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected || bleService == null)
        {
            removeDevice();
            watcher.Start();
        }
#endif
    }

    public async void Write(Guid characteristicToWriteToId, int value)
    {
#if WINDOWS_UWP
        Debug.Log($"write bleServive={bleService}");

        if (bleService != null)
        {
            var resultCharacteristics = await bleService.GetCharacteristicsForUuidAsync(characteristicToWriteToId);
            if (resultCharacteristics == null)
            {
                return;
            }
            GattCharacteristic characteristic = resultCharacteristics.Characteristics.FirstOrDefault();
            if (characteristic != null && characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
            {
                var writer = new DataWriter();
                writer.ByteOrder = ByteOrder.LittleEndian;
                writer.WriteInt32(value);


                try
                {
                    // BT_Code: Writes the value from the buffer to the characteristic.
                    var result = await characteristic.WriteValueWithResultAsync(writer.DetachBuffer());

                    if (result.Status != GattCommunicationStatus.Success)
                    {
                        Debug.Log($"Write failed: {result.Status}");
                    }
                }
                catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_INVALID_PDU)
                {
                    Debug.Log(ex.Message);
                    return;
                }
                catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED || ex.HResult == E_ACCESSDENIED)
                {
                    // This usually happens when a device reports that it support writing, but it actually doesn't.
                    Debug.Log(ex.Message);
                    return;
                }
            }
        }
#endif
    }*/


}