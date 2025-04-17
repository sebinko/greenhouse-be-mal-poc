import paho.mqtt.client as mqtt
import random
import time
import json
import logging

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger()

# MQTT Broker settings
MQTT_BROKER = "mosquitto"
MQTT_PORT = 1883
CLIENT_ID = f"greenhouse-simulator-{random.randint(0, 1000)}"

# Topics
SENSOR_TOPIC = "greenhouse/sensors"
CONTROL_TOPICS = {
    "watering": "greenhouse/control/watering",
    "light": "greenhouse/control/light",
    "fertilizer": "greenhouse/control/fertilizer"
}

class GreenhouseSimulator:
    def __init__(self):
        # Connect to MQTT broker
        self.client = mqtt.Client(CLIENT_ID)
        self.client.on_connect = self.on_connect
        self.client.on_disconnect = self.on_disconnect
        
        # Sensor ranges (min, max, precision)
        self.sensor_ranges = {
            "air_temperature": (15.0, 35.0, 1),  # °C
            "air_humidity": (40.0, 95.0, 1),     # %
            "soil_humidity": (20.0, 90.0, 1),    # %
            "light_level": (0, 1000, 0)          # lux
        }
        
        # Control states
        self.control_states = {
            "watering": False,
            "light": False,
            "fertilizer": False
        }
        
        # Control probabilities (chance of changing state per cycle)
        self.control_probabilities = {
            "watering": 0.1,     # 10% chance to toggle
            "light": 0.05,       # 5% chance to toggle
            "fertilizer": 0.02   # 2% chance to toggle
        }

    def connect(self):
        try:
            logger.info(f"Connecting to MQTT broker at {MQTT_BROKER}:{MQTT_PORT}")
            self.client.connect(MQTT_BROKER, MQTT_PORT)
            self.client.loop_start()
        except Exception as e:
            logger.error(f"Failed to connect to MQTT broker: {e}")
            raise

    def on_connect(self, client, userdata, flags, rc):
        if rc == 0:
            logger.info("Connected to MQTT broker")
        else:
            logger.error(f"Failed to connect to MQTT broker with code {rc}")

    def on_disconnect(self, client, userdata, rc):
        logger.warning(f"Disconnected from MQTT broker with code {rc}")

    def generate_sensor_data(self):
        data = {}
        for sensor, (min_val, max_val, precision) in self.sensor_ranges.items():
            # Generate random value within range
            value = random.uniform(min_val, max_val)
            # Apply precision
            if precision > 0:
                value = round(value, precision)
            else:
                value = int(value)
            data[sensor] = value
        return data

    def publish_sensor_data(self):
        """Publish simulated sensor data to MQTT broker"""
        sensor_data = self.generate_sensor_data()
        payload = json.dumps(sensor_data)
        
        try:
            self.client.publish(SENSOR_TOPIC, payload, qos=1)
            logger.info(f"Published sensor data: {payload}")
        except Exception as e:
            logger.error(f"Error publishing sensor data: {e}")

    def check_and_publish_control_states(self):
        """Randomly change and publish control states"""
        for control, topic in CONTROL_TOPICS.items():
            # Check if we should toggle this control
            if random.random() < self.control_probabilities[control]:
                # Toggle the state
                self.control_states[control] = not self.control_states[control]
                
                # Publish the new state
                state = "ON" if self.control_states[control] else "OFF"
                try:
                    self.client.publish(topic, state, qos=1, retain=True)
                    logger.info(f"Published {control} state: {state}")
                except Exception as e:
                    logger.error(f"Error publishing {control} state: {e}")

    def run(self, interval=10):
        """Run the simulation with the specified interval in seconds"""
        try:
            self.connect()
            logger.info(f"Starting greenhouse simulation, publishing every {interval} seconds")
            
            while True:
                self.publish_sensor_data()
                self.check_and_publish_control_states()
                time.sleep(interval)
                
        except KeyboardInterrupt:
            logger.info("Simulation stopped by user")
        finally:
            self.client.loop_stop()
            self.client.disconnect()
            logger.info("Disconnected from MQTT broker")

if __name__ == "__main__":
    simulator = GreenhouseSimulator()
    simulator.run(interval=5)  # Publish data every 5 seconds