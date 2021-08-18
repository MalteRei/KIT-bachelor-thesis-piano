#include <WiFi.h>
#include <WiFiClient.h>
#include <WebServer.h>
#include <ESPmDNS.h>

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

double tap_freq = 7;
double interval = (1.0/(tap_freq * 2))*1000.0;
double tap_duration = 300;
double number_of_tap_iterations = tap_duration / interval;

int zero = 0;

const TickType_t WaitTime = 5 / portTICK_PERIOD_MS;

xQueueHandle fingersToTriggerQueue;


WebServer server(80);

class Actuator {
  public:
    Actuator(){}
    Actuator(int gpio_pin_actuator_1, int gpio_pin_actuator_2)
      : gpio_pin_1(gpio_pin_actuator_1), gpio_pin_2(gpio_pin_actuator_2)
      {
        
   pinMode(gpio_pin_1, OUTPUT);
  pinMode(gpio_pin_2, OUTPUT);
        }
  
   void handleTrigger(){
     while(xQueueSend( fingersToTriggerQueue, (void*)this, WaitTime ) == errQUEUE_FULL){
        
      }
   }

    void play( ){
      int pin_1_on = LOW;
      Serial.println(gpio_pin_1);
      Serial.println(gpio_pin_2);
      for(int i = 0; i < 7; ++i){
        digitalWrite(gpio_pin_1, pin_1_on);
        digitalWrite(gpio_pin_2, 1 - pin_1_on);

        pin_1_on = pin_1_on == LOW?HIGH:LOW;
        
        delay(30);
      }
      digitalWrite(gpio_pin_1, LOW);
      digitalWrite(gpio_pin_2, LOW);
      delay(30);
    }

    

    private:
      int gpio_pin_1 {0};
      int gpio_pin_2 {0};
      
      
    
      
};

Actuator thumbCallback {THUMB_INPUT_1, THUMB_INPUT_2};
Actuator indexCallback {INDEX_INPUT_1, INDEX_INPUT_2};
Actuator middleCallback {MIDDLE_INPUT_1, MIDDLE_INPUT_2};
Actuator ringCallback {RING_INPUT_1, RING_INPUT_2};
Actuator pinkyCallback {PINKY_INPUT_1, PINKY_INPUT_2};

void handleThumbActuator(){
  thumbCallback.handleTrigger();
}


const char* ssid = "UPCE316243";
const char* password = "Defender!2007_2005";




void handleNotFound() {
  String message = "Not Found\n\n";
  message += "URI: ";
  message += server.uri();
  message += "\nMethod: ";
  message += (server.method() == HTTP_GET) ? "GET" : "POST";
  message += "\nArguments: ";
  message += server.args();
  message += "\n";
  for (uint8_t i = 0; i < server.args(); i++) {
    message += " " + server.argName(i) + ": " + server.arg(i) + "\n";
  }
  server.send(404, "text/plain", message);
}


 void playActuators(void * pvParameters){
   Actuator actuatorToCall;
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
void setupActuatorEnable(){
  
  pinMode(ENABLE, OUTPUT);
    digitalWrite(ENABLE, HIGH);
  
}

void setup(void) {
  Serial.begin(115200);

  setupActuatorEnable();

  fingersToTriggerQueue = xQueueCreate( 5, sizeof( Actuator ) );
  if( fingersToTriggerQueue == NULL )
  {
    ESP.restart();  
  }

  createTaskPlayingActuators();
  
  
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  Serial.println("");

  // Wait for connection
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.print("Connected to ");
  Serial.println(ssid);
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());

  if (MDNS.begin("esp32")) {
    Serial.println("MDNS responder started");
  }

  server.on("/hand/right/thumb", HTTP_POST, [](){
    thumbCallback.handleTrigger();
   server.send(202, "text/plain", "thumb actuator will be triggered");
  });

  server.on("/hand/right/index", HTTP_POST, [](){
    indexCallback.handleTrigger();
   server.send(202, "text/plain", "index actuator will be triggered");
  });

  server.on("/hand/right/middle", HTTP_POST, [](){
    middleCallback.handleTrigger();
   server.send(202, "text/plain", "middle actuator will be triggered");
  });

  server.on("/hand/right/ring", HTTP_POST, [](){
    ringCallback.handleTrigger();
   server.send(202, "text/plain", "ring actuator will be triggered");
  });

  server.on("/hand/right/pinky", HTTP_POST, [](){
    pinkyCallback.handleTrigger();
   server.send(202, "text/plain", "pinky actuator will be triggered");
  });
  

  

  server.onNotFound(handleNotFound);

  server.begin();
  Serial.println("HTTP server started");
}

void loop(void) {
  server.handleClient();
  delay(2);//allow the cpu to switch to other tasks
}
