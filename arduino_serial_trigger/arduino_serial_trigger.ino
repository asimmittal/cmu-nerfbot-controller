const int triggerPin = A2;
const int led = 13;

void setup(){
  pinMode(triggerPin,OUTPUT);
  pinMode(led,OUTPUT);
  digitalWrite(led,LOW);
  digitalWrite(triggerPin,LOW);
  Serial.begin(9600);
}

void fire(){
  digitalWrite(triggerPin,HIGH);
  digitalWrite(led,HIGH);
}

void halt(){
  digitalWrite(triggerPin,LOW);
  digitalWrite(led,LOW);
}

void loop(){
  
  int incomingByte = 0;
  
  if(Serial.available() > 0){
    incomingByte = Serial.read();
    if(incomingByte == 70 || incomingByte == 102)  fire();
    else if(incomingByte == 65 || incomingByte == 97) halt();
  }
  
}
