#/bin/sh

# export gpio pins for output (only has to be once).
#sudo echo 0 > /sys/class/gpio/export
#sudo echo out > /sys/class/gpio/gpio0/direction

#reset sam3x
sudo echo 0 > /sys/class/gpio/gpio0/value
sudo echo 1 > /sys/class/gpio/gpio0/value

exit 0
