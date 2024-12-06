#include <arduinoFFT.h>
#include <MD_MAX72xx.h>

// Set true to calibrate the EQ array values
constexpr bool CALIBRATION = true;

// Board configuration for Arduino Uno
constexpr int PIN_CS = 10;   // Chip Select for MAX7219
constexpr int PIN_CLK = 13;  // Clock for SPI (SCK)
constexpr int PIN_DIN = 11;  // Data Input (MOSI)
constexpr int PIN_MIC = A0;  // Microphone Analog Input

// MAX7219 configuration
constexpr int NUM_MATRIX = 4, NUM_DOT = 8;
static MD_MAX72XX disp = MD_MAX72XX(MD_MAX72XX::FC16_HW, PIN_DIN, PIN_CLK, PIN_CS, NUM_MATRIX);
constexpr int bar[] = {
  0b00000000,
  0b10000000,
  0b11000000,
  0b11100000,
  0b11110000,
  0b11111000,
  0b11111100,
  0b11111110,
  0b11111111,
};

// FFT configuration
constexpr int SAMPLES = 128, SAMPLING_FREQ = 32000;

// Display configuration
constexpr int NUM_BANDS = NUM_DOT * NUM_MATRIX, MAX_VALUE = 80;

// Amplifier and EQ
constexpr double AMPLITUDE = 4;
static double EQ[NUM_BANDS] = {
  1.10, 0.90, 0.31, 0.38, 0.28, 0.29, 0.44, 0.33, 0.50, 1.71, 1.98,
  0.38, 0.41, 0.48, 0.32, 0.32, 0.28, 0.23, 0.25, 0.96, 0.23, 0.12, 
  0.24, 0.43, 0.22, 0.30, 0.15, 0.55, 0.08, 0.11, 0.06, 0.08, 
};

static unsigned int sampling_period_us;
static double vReal[SAMPLES], vImag[SAMPLES], bands[NUM_BANDS];
static arduinoFFT fft = arduinoFFT(vReal, vImag, SAMPLES, SAMPLING_FREQ);

static void disp_init() {
  disp.begin();
  disp.control(MD_MAX72XX::UPDATE, 0);
  disp.control(MD_MAX72XX::INTENSITY, MAX_INTENSITY / 8);
}

static void eq_init() {
  for (int i = 0; i < NUM_BANDS; i++) {
    EQ[i] /= AMPLITUDE;
  }
}

void setup() {
  sampling_period_us = round(1000000 * (1.0 / SAMPLING_FREQ));
  eq_init();
  Serial.begin(9600);  // Initialize Serial Monitor
  disp_init();
}

static void do_sampling() {
  memset(vImag, 0, sizeof(vImag));
  for (int i = 0; i < SAMPLES; i++) {
    auto newTime = micros();
    vReal[i] = analogRead(PIN_MIC);
    while ((micros() - newTime) < sampling_period_us) {
      // Wait until the next sample
    }
  }
}

static void calc_fft() {
  fft.DCRemoval();
  fft.Windowing(FFTWindow::Hamming, FFTDirection::Forward);
  fft.Compute(FFTDirection::Forward);
  fft.ComplexToMagnitude();
}

// Map 48 FFT bins to 32 spectrum bands
static int bin_to_band(int bin) {
  switch (bin) {
    case 0 ... 25: return bin;
    case 26 ... 27: return 26;
    case 28 ... 29: return 27;
    case 30 ... 33: return 28;
    case 34 ... 37: return 29;
    case 38 ... 42: return 30;
    case 43 ... 47: return 31;
    default: return 31;
  }
}

static void collect_bands() {
  memset(bands, 0, sizeof(bands));
  for (int i = 0; i < 48; i++) {
    bands[bin_to_band(i)] += vReal[i];
  }
}

static void calibration() {
  static int cnt;
  static double band_max[NUM_BANDS];
  Serial.println(cnt);

  if (cnt++ < 1600) {
    for (int i = 0; i < NUM_BANDS; i++) {
      band_max[i] = max(band_max[i], bands[i]);
    }
  } else {
    Serial.println("-------------------------------");
    double base = MAX_VALUE * AMPLITUDE * 1.2;
    for (int i = 0; i < NUM_BANDS; i++) {
      char buffer[20];
      sprintf(buffer, " %f,", base / band_max[i]);
      Serial.print(buffer);
    }
    Serial.println("\n-------------------------------");
    cnt = 0;
    memset(band_max, 0, sizeof(band_max));
  }
}

static void post_processing() {
  if (CALIBRATION) {
    calibration();
  }
  for (int i = 0; i < NUM_BANDS; i++) {
    bands[i] *= EQ[i];
  }
}

static void display() {
  double max_v = 0;
  for (int i = 0; i < NUM_BANDS; i++) {
    max_v = max(max_v, bands[i]);
  }

  // Filter noise
  if (max_v < MAX_VALUE / NUM_DOT * 2) {
    memset(bands, 0, sizeof(bands));
  }
  max_v = max(max_v, MAX_VALUE);

  for (int i = 0; i < NUM_BANDS; i++) {
    disp.setColumn(NUM_BANDS - i - 1, bar[map(bands[i], 0, max_v, 0, 8)]);
  }
  disp.update();
}

void loop() {
  do_sampling();
  calc_fft();
  collect_bands();

  // Calculate and display mic volume
  double volume = 0;
  for (int i = 0; i < SAMPLES; i++) {
    volume += vReal[i];
  }
  volume /= SAMPLES;  // Average volume
  Serial.print("Mic Volume: ");
  Serial.println(volume);

  post_processing();
  display();
}
