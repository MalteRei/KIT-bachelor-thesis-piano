#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>

#define SERVICE_UUID        "00011000-e3c7-4819-9aeb-572d3c4d5b62"
//#define THUMB_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID "f2fcadbb-bb52-42fc-b88e-0d843cb57464"
#define THUMB_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID "00011001-e3c7-4819-9aeb-572d3c4d5b62"
#define INDEX_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID "00011002-e3c7-4819-9aeb-572d3c4d5b62"
#define MIDDLE_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID "00011003-e3c7-4819-9aeb-572d3c4d5b62"
#define RING_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID "00011004-e3c7-4819-9aeb-572d3c4d5b62"
#define PINKY_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID "00011005-e3c7-4819-9aeb-572d3c4d5b62"

#define ENABLE 2

#define THUMB_INPUT_1 12
#define THUMB_INPUT_2 13

#define INDEX_INPUT_1 26
#define INDEX_INPUT_2 27

#define MIDDLE_INPUT_1 32
#define MIDDLE_INPUT_2 33

#define RING_INPUT_1 22
#define RING_INPUT_2 23

#define PINKY_INPUT_1 4
#define PINKY_INPUT_2 5

const int freq = 3000;
const int pwmChannel = 0;
const int resolution = 8;
int dutyCycle = 200;

int zero = 0;

const TickType_t WaitTime = 5 / portTICK_PERIOD_MS;

xQueueHandle fingersToTriggerQueue;



class ActuatorVibrateCallbackCallbacks: public BLECharacteristicCallbacks {
  public:

     ActuatorVibrateCallbackCallbacks(){}
    ActuatorVibrateCallbackCallbacks(int gpio_pin_actuator_1, int gpio_pin_actuator_2)
      : gpio_pin_1(gpio_pin_actuator_1), gpio_pin_2(gpio_pin_actuator_2)
      {
        
   pinMode(gpio_pin_1, OUTPUT);
  pinMode(gpio_pin_2, OUTPUT);
        }
  
    void onWrite(BLECharacteristic *pCharacteristic) {
      uint8_t valueNum = *(pCharacteristic->getData());
       if(valueNum == 1){
        triggerActuator();
          }

      
    }

    void play( ){
      Serial.println("Play()");
      for(int i = 0; i < 5; ++i){
        digitalWrite(gpio_pin_1, LOW);
        digitalWrite(gpio_pin_2, HIGH);
      
        delay(30);
      
        digitalWrite(gpio_pin_1, HIGH);
        digitalWrite(gpio_pin_2, LOW);
        
        delay(30);
      }
      digitalWrite(gpio_pin_1, LOW);
      digitalWrite(gpio_pin_2, LOW);
      delay(30);
      Serial.println("done Play()");
    }

    void triggerActuator(){

      while(xQueueSend( fingersToTriggerQueue, (void*)this, WaitTime ) == errQUEUE_FULL){
        
      }
    }

    private:
      int gpio_pin_1 {0};
      int gpio_pin_2 {0};
      
      
    
      
};

void callPlayActuator(void * pvParameters){
  ActuatorVibrateCallbackCallbacks* actuator = (ActuatorVibrateCallbackCallbacks*) pvParameters;
  if(actuator != NULL){
    actuator->play( );
  }
}


void setupCharacteristicReadWrite(BLEService *pService, const char* characteristicUUID, BLECharacteristicCallbacks* pCallbacks) {
    BLECharacteristic *pCharacteristic = pService->createCharacteristic(
                                         characteristicUUID,
                                         BLECharacteristic::PROPERTY_READ |
                                         BLECharacteristic::PROPERTY_WRITE
                                       );
  pCharacteristic->setCallbacks(pCallbacks);
    pCharacteristic->setValue(zero);
}


ActuatorVibrateCallbackCallbacks thumbCallback {THUMB_INPUT_1, THUMB_INPUT_2};
ActuatorVibrateCallbackCallbacks indexCallback {INDEX_INPUT_1, INDEX_INPUT_2};
ActuatorVibrateCallbackCallbacks middleCallback {MIDDLE_INPUT_1, MIDDLE_INPUT_2};
ActuatorVibrateCallbackCallbacks ringCallback {RING_INPUT_1, RING_INPUT_2};
ActuatorVibrateCallbackCallbacks pinkyCallback {PINKY_INPUT_1, PINKY_INPUT_2};

void setup() {
  Serial.begin(115200);
  // put your setup code here, to run once:
  setupActuator();

  fingersToTriggerQueue = xQueueCreate( 5, sizeof( ActuatorVibrateCallbackCallbacks ) );
if( fingersToTriggerQueue == NULL )
{
  ESP.restart();  
}
  createTaskPlayingActuators();
  
  BLEDevice::init("RightHandActuators");
  BLEServer *pServer = BLEDevice::createServer();

  
  BLEService *pService = pServer->createService(SERVICE_UUID);

  setupCharacteristicReadWrite(pService, THUMB_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID, &thumbCallback);
  setupCharacteristicReadWrite(pService, INDEX_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID, &indexCallback);
  setupCharacteristicReadWrite(pService, MIDDLE_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID, &middleCallback);
  setupCharacteristicReadWrite(pService, RING_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID, &ringCallback);
  setupCharacteristicReadWrite(pService, PINKY_ACTUATOR_VIBRATE_CHARACTERISTIC_UUID, &pinkyCallback);

   pService->start();

  BLEAdvertising *pAdvertising = pServer->getAdvertising();
  pAdvertising->start();

  Serial.println("started ble");
 }

 void playActuators(void * pvParameters){
   ActuatorVibrateCallbackCallbacks actuatorToCall;
  for(;;){
      
       if (xQueueReceive( fingersToTriggerQueue, &actuatorToCall, WaitTime ) == pdPASS)
      {
        actuatorToCall.play();
      }
  }
}


 void createTaskPlayingActuators(){
    TaskHandle_t xHandle = NULL;
      xTaskCreatePinnedToCore(
          playActuators, /* Function to implement the task */
          "Task1", /* Name of the task */
          10000,  /* Stack size in words */
          NULL, /* Task input parameter */
          0,  /* Priority of the task */
          &xHandle,  /* Task handle. */
          0); /* Core where the task should run */

    
 }

void setupActuator(){
  pinMode(ENABLE, OUTPUT);
    digitalWrite(ENABLE, HIGH);
    /*
  ledcSetup(pwmChannel, freq, resolution);
  ledcAttachPin(ENABLE, pwmChannel);
   ledcWrite(pwmChannel, 255);  */
}

void loop() {
  // put your main code here, to run repeatedly:
   delay(2000);
}
