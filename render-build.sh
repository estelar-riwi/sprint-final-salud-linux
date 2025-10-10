#!/bin/bash

# Actualiza lista de paquetes e instala libgdiplus para System.Drawing en Linux
apt-get update
apt-get install -y libgdiplus
apt-get install -y libc6-dev

# Enlazar libgdiplus (por si acaso)
ln -s /usr/lib/libgdiplus.so /usr/lib/libgdiplus.so

