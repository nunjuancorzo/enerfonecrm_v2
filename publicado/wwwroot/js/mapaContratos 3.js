window.mapaContratos = {
    mapa: null,
    marcadores: [],

    // Coordenadas de las capitales de provincia españolas
    coordenadasProvincias: {
        "01": { lat: 42.8467, lng: -2.6722, nombre: "Álava" },
        "02": { lat: 38.9943, lng: -1.8585, nombre: "Albacete" },
        "03": { lat: 38.3452, lng: -0.4810, nombre: "Alicante" },
        "04": { lat: 36.8381, lng: -2.4597, nombre: "Almería" },
        "05": { lat: 40.6561, lng: -4.6989, nombre: "Ávila" },
        "06": { lat: 38.8794, lng: -6.9706, nombre: "Badajoz" },
        "07": { lat: 39.5696, lng: 2.6502, nombre: "Baleares" },
        "08": { lat: 41.3874, lng: 2.1686, nombre: "Barcelona" },
        "09": { lat: 42.3439, lng: -3.6969, nombre: "Burgos" },
        "10": { lat: 39.4753, lng: -6.3724, nombre: "Cáceres" },
        "11": { lat: 36.5270, lng: -6.2886, nombre: "Cádiz" },
        "12": { lat: 39.9864, lng: -0.0513, nombre: "Castellón" },
        "13": { lat: 38.9848, lng: -3.9271, nombre: "Ciudad Real" },
        "14": { lat: 37.8882, lng: -4.7794, nombre: "Córdoba" },
        "15": { lat: 43.3623, lng: -8.4115, nombre: "A Coruña" },
        "16": { lat: 40.0704, lng: -2.1374, nombre: "Cuenca" },
        "17": { lat: 41.9794, lng: 2.8214, nombre: "Girona" },
        "18": { lat: 37.1773, lng: -3.5986, nombre: "Granada" },
        "19": { lat: 40.6324, lng: -3.1644, nombre: "Guadalajara" },
        "20": { lat: 43.3183, lng: -1.9812, nombre: "Gipuzkoa" },
        "21": { lat: 37.2614, lng: -6.9447, nombre: "Huelva" },
        "22": { lat: 42.1401, lng: -0.4079, nombre: "Huesca" },
        "23": { lat: 37.7796, lng: -3.7849, nombre: "Jaén" },
        "24": { lat: 42.5987, lng: -5.5671, nombre: "León" },
        "25": { lat: 41.6176, lng: 0.6200, nombre: "Lleida" },
        "26": { lat: 42.2871, lng: -2.5396, nombre: "La Rioja" },
        "27": { lat: 43.0097, lng: -7.5567, nombre: "Lugo" },
        "28": { lat: 40.4168, lng: -3.7038, nombre: "Madrid" },
        "29": { lat: 36.7213, lng: -4.4214, nombre: "Málaga" },
        "30": { lat: 37.9922, lng: -1.1307, nombre: "Murcia" },
        "31": { lat: 42.8125, lng: -1.6458, nombre: "Navarra" },
        "32": { lat: 42.3405, lng: -7.8632, nombre: "Ourense" },
        "33": { lat: 43.3614, lng: -5.8593, nombre: "Asturias" },
        "34": { lat: 42.0098, lng: -4.5288, nombre: "Palencia" },
        "35": { lat: 28.1236, lng: -15.4366, nombre: "Las Palmas" },
        "36": { lat: 42.4333, lng: -8.6500, nombre: "Pontevedra" },
        "37": { lat: 40.9701, lng: -5.6635, nombre: "Salamanca" },
        "38": { lat: 28.4698, lng: -16.2546, nombre: "Santa Cruz de Tenerife" },
        "39": { lat: 43.1828, lng: -3.9878, nombre: "Cantabria" },
        "40": { lat: 40.9429, lng: -4.1088, nombre: "Segovia" },
        "41": { lat: 37.3891, lng: -5.9845, nombre: "Sevilla" },
        "42": { lat: 41.7665, lng: -2.4790, nombre: "Soria" },
        "43": { lat: 41.1189, lng: 1.2445, nombre: "Tarragona" },
        "44": { lat: 40.3456, lng: -1.1065, nombre: "Teruel" },
        "45": { lat: 39.8628, lng: -4.0273, nombre: "Toledo" },
        "46": { lat: 39.4699, lng: -0.3763, nombre: "Valencia" },
        "47": { lat: 41.6520, lng: -4.7245, nombre: "Valladolid" },
        "48": { lat: 43.2630, lng: -2.9350, nombre: "Bizkaia" },
        "49": { lat: 41.5034, lng: -5.7467, nombre: "Zamora" },
        "50": { lat: 41.6488, lng: -0.8891, nombre: "Zaragoza" },
        "51": { lat: 35.8894, lng: -5.3213, nombre: "Ceuta" },
        "52": { lat: 35.2923, lng: -2.9381, nombre: "Melilla" }
    },

    inicializar: function (elementId) {
        if (this.mapa) {
            this.mapa.remove();
        }

        // Crear mapa centrado en España
        this.mapa = L.map(elementId).setView([40.4168, -3.7038], 6);

        // Añadir capa de tiles (OpenStreetMap)
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors',
            maxZoom: 18
        }).addTo(this.mapa);

        return true;
    },

    agregarMarcadores: function (provincias) {
        // Limpiar marcadores anteriores
        this.marcadores.forEach(m => m.remove());
        this.marcadores = [];

        // Convertir a array si es necesario
        const provinciasArray = Array.isArray(provincias) ? provincias : [provincias];

        provinciasArray.forEach(provincia => {
            const coords = this.coordenadasProvincias[provincia.codigoPostal];
            if (!coords) return;

            // Crear icono personalizado según el tipo predominante
            let color = '#6c757d'; // gris por defecto
            let iconoTipo = '📍';
            
            if (provincia.energia > provincia.telefonia && provincia.energia > provincia.alarmas) {
                color = '#ffc107'; // amarillo para energía
                iconoTipo = '⚡';
            } else if (provincia.telefonia > provincia.energia && provincia.telefonia > provincia.alarmas) {
                color = '#0d6efd'; // azul para telefonía
                iconoTipo = '📞';
            } else if (provincia.alarmas > 0) {
                color = '#dc3545'; // rojo para alarmas
                iconoTipo = '🛡️';
            }

            // Crear icono HTML personalizado
            const iconHtml = `
                <div style="
                    background-color: ${color};
                    width: 30px;
                    height: 30px;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    border: 2px solid white;
                    box-shadow: 0 2px 5px rgba(0,0,0,0.3);
                    font-size: 14px;
                ">
                    ${iconoTipo}
                </div>
            `;

            const customIcon = L.divIcon({
                html: iconHtml,
                className: 'custom-marker',
                iconSize: [30, 30],
                iconAnchor: [15, 15],
                popupAnchor: [0, -15]
            });

            // Crear popup con información
            const popupContent = `
                <div style="min-width: 200px;">
                    <h6 style="margin-bottom: 10px; font-weight: bold;">${coords.nombre}</h6>
                    <div style="margin-bottom: 5px;">
                        <span style="color: #ffc107;">⚡</span> Energía: <strong>${provincia.energia}</strong>
                    </div>
                    <div style="margin-bottom: 5px;">
                        <span style="color: #0d6efd;">📞</span> Telefonía: <strong>${provincia.telefonia}</strong>
                    </div>
                    <div style="margin-bottom: 5px;">
                        <span style="color: #dc3545;">🛡️</span> Alarmas: <strong>${provincia.alarmas}</strong>
                    </div>
                    <hr style="margin: 10px 0;">
                    <div style="font-weight: bold; font-size: 1.1em;">
                        Total: ${provincia.total} contratos
                    </div>
                </div>
            `;

            // Crear y añadir marcador
            const marcador = L.marker([coords.lat, coords.lng], { icon: customIcon })
                .bindPopup(popupContent)
                .addTo(this.mapa);

            this.marcadores.push(marcador);
        });

        return true;
    },

    agregarMarcadoresIndividuales: function (contratos) {
        // Limpiar marcadores anteriores
        this.marcadores.forEach(m => m.remove());
        this.marcadores = [];

        // Convertir a array si es necesario
        const contratosArray = Array.isArray(contratos) ? contratos : [contratos];
        
        console.log(`Renderizando ${contratosArray.length} contratos en el mapa`);

        contratosArray.forEach(async (contrato) => {
            // Construir dirección completa para geocodificación
            let direccionCompleta = '';
            
            if (contrato.direccion) {
                direccionCompleta += contrato.direccion + ', ';
            }
            if (contrato.localidad) {
                direccionCompleta += contrato.localidad + ', ';
            }
            if (contrato.provincia) {
                direccionCompleta += contrato.provincia + ', ';
            }
            if (contrato.codigoPostal) {
                direccionCompleta += contrato.codigoPostal + ', ';
            }
            direccionCompleta += 'España';

            console.log(`Geocodificando contrato ID ${contrato.id}: ${direccionCompleta}`);

            // Intentar geocodificar la dirección usando Nominatim
            try {
                const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(direccionCompleta)}&limit=1&countrycodes=es`;
                const response = await fetch(url);
                const data = await response.json();

                let lat, lng;
                
                if (data && data.length > 0) {
                    // Si la geocodificación fue exitosa, usar esas coordenadas
                    lat = parseFloat(data[0].lat);
                    lng = parseFloat(data[0].lon);
                    console.log(`✓ Geocodificación exitosa ID ${contrato.id}: ${lat}, ${lng}`);
                } else {
                    // Si falla, usar coordenadas de provincia como respaldo
                    console.warn(`⚠ No se pudo geocodificar ID ${contrato.id}: ${direccionCompleta}. Usando provincia como respaldo.`);
                    const prefijoCp = contrato.codigoPostal ? contrato.codigoPostal.substring(0, 2) : '28';
                    const coords = this.coordenadasProvincias[prefijoCp];
                    if (!coords) {
                        console.warn(`✗ No se encontraron coordenadas para CP ${prefijoCp}`);
                        return;
                    }
                    // Añadir pequeña variación aleatoria
                    lat = coords.lat + (Math.random() - 0.5) * 0.3;
                    lng = coords.lng + (Math.random() - 0.5) * 0.3;
                }

                // Determinar color según tipo de contrato
                let color = '#6c757d';
                let iconoTipo = '📍';
                
                if (contrato.tipo === 'energia') {
                    color = '#ffc107'; // amarillo
                    iconoTipo = '⚡';
                } else if (contrato.tipo === 'telefonia') {
                    color = '#0d6efd'; // azul
                    iconoTipo = '📞';
                } else if (contrato.tipo === 'alarma') {
                    color = '#dc3545'; // rojo
                    iconoTipo = '🛡️';
                }

                // Crear marcador circular
                const circleMarker = L.circleMarker([lat, lng], {
                    radius: 6,
                    fillColor: color,
                    color: '#fff',
                    weight: 2,
                    opacity: 1,
                    fillOpacity: 0.8
                });

                // Crear popup con información del contrato
                const popupContent = `
                    <div style="min-width: 200px;">
                        <div style="margin-bottom: 8px;">
                            <span style="font-size: 1.2em;">${iconoTipo}</span>
                            <strong style="margin-left: 5px;">${contrato.tipo === 'energia' ? 'Energía' : contrato.tipo === 'telefonia' ? 'Telefonía' : 'Alarma'}</strong>
                        </div>
                        <div style="margin-bottom: 5px;">
                            <strong>Cliente:</strong> ${contrato.cliente}
                        </div>
                        ${contrato.direccion ? `<div style="margin-bottom: 5px;"><strong>Dirección:</strong> ${contrato.direccion}</div>` : ''}
                        ${contrato.localidad ? `<div style="margin-bottom: 5px;"><strong>Localidad:</strong> ${contrato.localidad}</div>` : ''}
                        ${contrato.provincia ? `<div style="margin-bottom: 5px;"><strong>Provincia:</strong> ${contrato.provincia}</div>` : ''}
                        ${contrato.codigoPostal ? `<div style="margin-bottom: 5px;"><strong>CP:</strong> ${contrato.codigoPostal}</div>` : ''}
                        <div style="margin-bottom: 5px;">
                            <strong>Estado:</strong> <span class="badge" style="background-color: ${color}; color: white; padding: 2px 6px; border-radius: 3px;">${contrato.estado}</span>
                        </div>
                        <div style="font-size: 0.85em; color: #666; margin-top: 5px;">
                            ${contrato.fecha}
                        </div>
                    </div>
                `;

                circleMarker.bindPopup(popupContent);
                circleMarker.addTo(this.mapa);
                
                this.marcadores.push(circleMarker);

                // Delay para no saturar el servicio de geocodificación (1 segundo entre peticiones)
                await new Promise(resolve => setTimeout(resolve, 1000));
                
            } catch (error) {
                console.error(`✗ Error al geocodificar contrato ID ${contrato.id} (${direccionCompleta}):`, error);
                // En caso de error, usar coordenadas de provincia como respaldo
                const prefijoCp = contrato.codigoPostal ? contrato.codigoPostal.substring(0, 2) : '28';
                const coords = this.coordenadasProvincias[prefijoCp];
                if (coords) {
                    const lat = coords.lat + (Math.random() - 0.5) * 0.3;
                    const lng = coords.lng + (Math.random() - 0.5) * 0.3;
                    
                    let color = contrato.tipo === 'energia' ? '#ffc107' : contrato.tipo === 'telefonia' ? '#0d6efd' : '#dc3545';
                    const circleMarker = L.circleMarker([lat, lng], {
                        radius: 6,
                        fillColor: color,
                        color: '#fff',
                        weight: 2,
                        opacity: 1,
                        fillOpacity: 0.8
                    });
                    circleMarker.addTo(this.mapa);
                    this.marcadores.push(circleMarker);
                }
            }
        });

        return true;
    },

    destruir: function () {
        if (this.mapa) {
            this.mapa.remove();
            this.mapa = null;
        }
        this.marcadores = [];
    }
};
