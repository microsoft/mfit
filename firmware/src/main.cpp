#include "WProgram.h"
#include "usb_desc.h"
#include "usb_dev.h"
#include "usb_rawhid.h"

#include <Arduino.h>

#ifndef USING_MAKEFILE
#error we only support Makefile
#endif

#define HID_TX_TIMEOUT_MS 1

static uint8_t comm_buf[64];
static void
send(void) {
	RawHID.send(&comm_buf[0], HID_TX_TIMEOUT_MS);
}
static void
recv(void)
{
	RawHID.recv(&comm_buf[0], HID_TX_TIMEOUT_MS);
}

#define PIN_LED_TEENSY    13

#define PIN_LED_USR       2
#define PIN_LED_ARMED     3
#define PIN_LED_FI        4
#define PIN_LED_DIS_ALRTN 5

#define PIN_LED PIN_LED_TEENSY

/* these are all configs, e.g. wires setting up, the numbers are
 * are the labels on the PCB
 */
#define PIN_CTRL1 32 /* INJ0_REF_CMD */
#define PIN_CTRL2 31 /* ALERTN_ENA_CMD */
#define PIN_CTRL3 30
#define PIN_TRIG  27

typedef struct __attribute__((packed)) command {
	uint8_t op;
	uint8_t pin;
	union {
		/* param 1 */
		uint32_t delay;
	};
	union {
		/* param 2 */
		uint32_t cnt;
	};
} cmd_t;

typedef struct __attribute__((packed)) response {
	uint8_t code;
	uint8_t payload_len;
	uint8_t payload[0];
} response_t;

static void inline
wait_for_trigger(void)
{
	/* the SDA bus has a pullup */
	while (digitalReadFast(PIN_TRIG)) {}
}

static uint8_t stat_identify = 0;
static cmd_t *cmd = NULL;
static response_t *resp = NULL;
extern "C" int main(void)
{

	//ARM_DEMCR |= ARM_DEMCR_TRCENA;
	//ARM_DWT_CTRL |= ARM_DWT_CTRL_CYCCNTENA;

	/* setup */
	pinMode(PIN_LED, OUTPUT);

	pinMode(PIN_LED_USR, OUTPUT);
	pinMode(PIN_LED_ARMED, OUTPUT);
	pinMode(PIN_LED_FI, OUTPUT);
	pinMode(PIN_LED_DIS_ALRTN, OUTPUT);

	pinMode(PIN_CTRL1, OUTPUT);
	pinMode(PIN_CTRL2, OUTPUT);

	pinMode(PIN_TRIG, INPUT);


	/* alertn is PIN_CTRL2 */
	/* inj is PIN_CTRL1 */
	digitalWriteFast(PIN_CTRL1, LOW);
	digitalWriteFast(PIN_CTRL2, LOW);

	/* by default, ALERTn is disabled */
	digitalWriteFast(PIN_LED_DIS_ALRTN, HIGH);


	cmd = (cmd_t *)&comm_buf[0];
	resp = (response_t *)&comm_buf[0];
#define HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER 50

/* we should get at least one refresh in this interval */
#define HARDCODED_DELAY_US_FOR_RECOVERY 10
#define SETH(pin) digitalWriteFast(pin, HIGH)
#define SETL(pin) digitalWriteFast(pin, LOW)
	while (1) {
		if (cmd->pin != 1 && cmd->pin != 2) {
			goto invalid_pin;
		}
		{
			const uint8_t pin = (cmd->pin == 1) ? PIN_CTRL1 : PIN_CTRL2;
			switch ((unsigned char)cmd->op) {
			case 'h':
					  if (pin == PIN_CTRL2) SETL(PIN_LED_DIS_ALRTN);
					  if (pin == PIN_CTRL1) SETH(PIN_LED_FI);
					  SETH(pin);
					  break;
			case 'l': SETL(pin);
					  if (pin == PIN_CTRL2) SETH(PIN_LED_DIS_ALRTN);
					  if (pin == PIN_CTRL1) SETL(PIN_LED_FI);
					  break;

			case 'd': SETH(PIN_LED_FI); SETH(pin); delay(cmd->delay); SETL(pin); SETL(PIN_LED_FI); break;
			case 'D': SETH(PIN_LED_FI); SETH(pin); delayMicroseconds(cmd->delay); SETL(pin); SETL(PIN_LED_FI); break;

			case 'v':
					  resp->code = 0;
					  resp->payload_len = 5;
					  resp->payload[0] = 'v';
					  resp->payload[1] = 'e';
					  resp->payload[2] = 'r';
					  resp->payload[3] = '2';
					  resp->payload[4] = '3';
					  send();
					  break;
			case 'w': SETH(PIN_LED_ARMED); wait_for_trigger(); SETL(PIN_LED_ARMED); break;

			case 't': SETH(PIN_LED_ARMED); wait_for_trigger(); SETH(PIN_LED_FI);
					  SETH(pin);
					  delay(cmd->delay);
					  SETL(pin);
					  SETL(PIN_LED_FI); SETL(PIN_LED_ARMED);
					  break;
			case 'T': SETH(PIN_LED_ARMED); wait_for_trigger(); SETH(PIN_LED_FI);
					  SETH(pin);
					  delayMicroseconds(cmd->delay);
					  SETL(pin);
					  SETL(PIN_LED_FI); SETL(PIN_LED_ARMED);
					  break;

			case 'a': SETH(PIN_LED_ARMED); wait_for_trigger(); SETH(PIN_LED_FI);
					  SETH(pin);
					  delay(cmd->delay);
					  wait_for_trigger();
					  SETL(pin);
					  SETL(PIN_LED_FI); SETL(PIN_LED_ARMED);
					  break;
			case 'A': SETH(PIN_LED_ARMED); wait_for_trigger(); SETH(PIN_LED_FI);
					  SETH(pin);
					  delayMicroseconds(cmd->delay);
					  wait_for_trigger();
					  SETL(pin);
					  SETL(PIN_LED_FI); SETL(PIN_LED_ARMED);
					  break;

			case 'm':
					  SETH(PIN_LED_ARMED);
					  wait_for_trigger();
					  while (cmd->cnt--) {
						  SETH(PIN_LED_FI);
						  SETH(pin);
						  delayMicroseconds(HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER);
						  wait_for_trigger();
						  SETL(pin);
						  SETL(PIN_LED_FI);
						  delay(cmd->delay);
					  }
					  SETL(PIN_LED_ARMED);
					  break;
			case 'M':
					  SETH(PIN_LED_ARMED);
					  wait_for_trigger();
					  while (cmd->cnt--) {
						  SETH(PIN_LED_FI);
						  SETH(pin);
						  delayMicroseconds(HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER);
						  wait_for_trigger();
						  SETL(pin);
						  SETL(PIN_LED_FI);
						  delayMicroseconds(cmd->delay);
					  }
					  SETL(PIN_LED_ARMED);
					  break;

			case 'r':
					  /* like 'a' but do it in a loop */
					  SETH(PIN_LED_ARMED);
					  while (cmd->cnt--) {
						  wait_for_trigger();
						  SETH(PIN_LED_FI);
						  SETH(pin);
						  delayMicroseconds(HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER);
						  wait_for_trigger();
						  SETL(pin);
						  SETL(PIN_LED_FI);
						  /* this delay should be at least
						   * HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER
						   * to not sense twice the same trigger sig */
						  delay(cmd->delay);
					  }
					  SETL(PIN_LED_ARMED);
					  break;
			case 'R':
					  /* like 'A' but do it in a loop */
					  SETH(PIN_LED_ARMED);
					  while (cmd->cnt--) {
						  wait_for_trigger();
						  SETH(PIN_LED_FI);
						  SETH(pin);
						  delayMicroseconds(HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER);
						  wait_for_trigger();
						  SETL(pin);
						  SETL(PIN_LED_FI);
						  /* this delay should be at least
						   * HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER
						   * to not sense twice the same trigger sig */
						  delayMicroseconds(cmd->delay);
					  }
					  SETL(PIN_LED_ARMED);
					  break;

			case 'f':
					  /* like 'f' but embedd the recovery sequence */
					  SETH(PIN_LED_ARMED);
					  while (cmd->cnt--) {
						  wait_for_trigger();
						  SETH(PIN_LED_FI);
						  SETH(pin);
						  delayMicroseconds(HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER);
						  wait_for_trigger();
						  SETL(pin);
						  SETL(PIN_LED_FI);
						  /* this delay should be at least
						   * HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER
						   * to not sense twice the same trigger sig */
						  delay(cmd->delay);

						  /* Recovery sequence */

						  /* enable alertn */
						  SETH(PIN_CTRL2);
						  SETH(PIN_LED_USR);

						  delayMicroseconds(1);

						  /* do FI */
						  SETH(PIN_LED_FI);
						  SETH(PIN_CTRL1);

						  delayMicroseconds(HARDCODED_DELAY_US_FOR_RECOVERY);

						  /* stop FI */
						  SETL(PIN_LED_FI);
						  SETL(PIN_CTRL1);

						  delayMicroseconds(1);

						  /* disable alertn */
						  SETL(PIN_CTRL2);
						  SETL(PIN_LED_USR);

					  }
					  SETL(PIN_LED_ARMED);
					  break;
			case 'F':
					  /* like 'R' but embedd the recovery sequence */
					  SETH(PIN_LED_ARMED);
					  while (cmd->cnt--) {
						  wait_for_trigger();
						  SETH(PIN_LED_FI);
						  SETH(pin);
						  delayMicroseconds(HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER);
						  wait_for_trigger();
						  SETL(pin);
						  SETL(PIN_LED_FI);
						  /* this delay should be at least
						   * HARDCODED_MINIMUM_DELAY_US_TO_SENSE_ANOTHER_TRIGGER
						   * to not sense twice the same trigger sig */
						  delayMicroseconds(cmd->delay);

						  /* Recovery sequence */

						  /* enable alertn */
						  SETH(PIN_CTRL2);
						  SETH(PIN_LED_USR);

						  delayMicroseconds(1);

						  /* do FI */
						  SETH(PIN_LED_FI);
						  SETH(PIN_CTRL1);

						  delayMicroseconds(HARDCODED_DELAY_US_FOR_RECOVERY);

						  /* stop FI */
						  SETL(PIN_LED_FI);
						  SETL(PIN_CTRL1);

						  delayMicroseconds(1);

						  /* disable alertn */
						  SETL(PIN_CTRL2);
						  SETL(PIN_LED_USR);
					  }
					  SETL(PIN_LED_ARMED);
					  break;

			case 'i':
					  stat_identify = !stat_identify;
					  if (stat_identify) {
						  SETH(PIN_LED_TEENSY);
						  if (cmd->pin == 2) {
							  SETH(PIN_LED_USR);
							  SETH(PIN_LED_ARMED);
							  SETH(PIN_LED_FI);
							  SETH(PIN_LED_DIS_ALRTN);
						  }
					  } else {
						  SETL(PIN_LED_TEENSY);
						  if (cmd->pin == 2) {
							  SETL(PIN_LED_USR);
							  SETL(PIN_LED_ARMED);
							  SETL(PIN_LED_FI);
							  SETL(PIN_LED_DIS_ALRTN);
						  }
					  }
					  break;

			case 'U': asm("bkpt #251"); break;
			default: break;
			};
		}
invalid_pin:
		cmd->op = 0;
		recv();
	}
	return 0;
}
