CREATE TABLE IF NOT EXISTS contrato_estado_servicio (
    id INT NOT NULL AUTO_INCREMENT,
    contrato_id INT NOT NULL,
    clave_linea VARCHAR(40) NOT NULL,
    estado VARCHAR(40) NOT NULL DEFAULT '',
    fecha_instalacion DATE NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_contrato_estado_servicio_linea (contrato_id, clave_linea),
    CONSTRAINT fk_contrato_estado_servicio_contrato
        FOREIGN KEY (contrato_id) REFERENCES contratos (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;