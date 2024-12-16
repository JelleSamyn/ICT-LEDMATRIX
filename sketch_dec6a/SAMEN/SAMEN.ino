#include <MD_MAX72xx.h>
#include <SPI.h>
#include <arduinoFFT.h>

void setup() {
  setup1();  // Initialize the FFT setup
  setup2();  // Initialize the scrolling display setup
}

void loop() {
  Serial.println("Running loop1...");
  loop1();

  Serial.println("Running loop2...");
  loop2();
}
