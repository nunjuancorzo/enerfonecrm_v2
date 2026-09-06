-- =====================================================
-- Separación de tarifas de energía en LUZ y GAS
-- Paso 1: creación de las tablas tarifasluz y tarifasgas
-- La columna "tipo de tarifa (LUZ/GAS)" desaparece:
-- queda implícita en la tabla correspondiente.
-- =====================================================

-- -----------------------------------------------------
-- TARIFAS DE LUZ
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS tarifasluz (
    id INT NOT NULL AUTO_INCREMENT,

    -- Identificación
    empresa VARCHAR(255) NOT NULL,              -- Compañía / comercializadora
    tipo_cliente VARCHAR(50) NOT NULL,          -- Residencial / Pyme / Autonomo
    nombre VARCHAR(255) NOT NULL,               -- Nombre comercial de la tarifa
    peaje VARCHAR(50) NULL,                     -- 2.0 / 3.0 / 6.1 / 6.2

    -- Precios de potencia (€/kW día) P1..P6
    potencia1 VARCHAR(255) NULL,
    potencia2 VARCHAR(255) NULL,
    potencia3 VARCHAR(255) NULL,
    potencia4 VARCHAR(255) NULL,
    potencia5 VARCHAR(255) NULL,
    potencia6 VARCHAR(255) NULL,

    -- Precios de energía (€/kWh) P1..P6
    energia1 VARCHAR(255) NULL,
    energia2 VARCHAR(255) NULL,
    energia3 VARCHAR(255) NULL,
    energia4 VARCHAR(255) NULL,
    energia5 VARCHAR(255) NULL,
    energia6 VARCHAR(255) NULL,

    -- Valores numéricos para el comparador
    termino_fijo_diario DECIMAL(18,6) NULL,
    precio_potencia_p1 DECIMAL(18,6) NULL,
    precio_potencia_p2 DECIMAL(18,6) NULL,
    precio_potencia_p3 DECIMAL(18,6) NULL,
    precio_energia_p1 DECIMAL(18,6) NULL,
    precio_energia_p2 DECIMAL(18,6) NULL,
    precio_energia_p3 DECIMAL(18,6) NULL,

    -- Condiciones comerciales
    descuento VARCHAR(255) NULL,
    observaciones_descuentos VARCHAR(500) NULL,
    excedentes VARCHAR(100) NULL,               -- Precio compensación excedentes
    bateria_virtual VARCHAR(50) NULL,
    permanencia VARCHAR(100) NULL,
    dias_penalizacion INT NULL,
    tipo_penalizacion VARCHAR(20) NULL,         -- Total / Proporcional

    -- Comisión y estado
    comision DECIMAL(18,2) NOT NULL DEFAULT 0,
    precioNew DECIMAL(18,2) NOT NULL DEFAULT 0,
    activa TINYINT(1) NOT NULL DEFAULT 1,
    fecha_carga DATETIME NULL,

    PRIMARY KEY (id),
    INDEX idx_tarifasluz_empresa (empresa),
    INDEX idx_tarifasluz_tipo_cliente (tipo_cliente),
    INDEX idx_tarifasluz_peaje (peaje),
    INDEX idx_tarifasluz_activa (activa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------
-- TARIFAS DE GAS
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS tarifasgas (
    id INT NOT NULL AUTO_INCREMENT,

    -- Identificación
    empresa VARCHAR(255) NOT NULL,              -- Compañía / comercializadora
    tipo_cliente VARCHAR(50) NOT NULL,          -- Residencial / Pyme / Autonomo
    nombre VARCHAR(255) NOT NULL,               -- Nombre comercial de la tarifa
    peaje_gas VARCHAR(50) NULL,                 -- RL.1 / RL.2 / RL.3 / RL.4 ...

    -- Precios
    termino_fijo_gas VARCHAR(255) NULL,         -- €/mes o €/día
    termino_variable_gas VARCHAR(255) NULL,     -- €/kWh
    pvd_sva VARCHAR(255) NULL,                  -- Peaje variable distribución / SVA

    -- Condiciones comerciales
    descuento VARCHAR(255) NULL,
    observaciones_descuentos VARCHAR(500) NULL,
    permanencia VARCHAR(100) NULL,
    dias_penalizacion INT NULL,
    tipo_penalizacion VARCHAR(20) NULL,         -- Total / Proporcional

    -- Comisión y estado
    comision DECIMAL(18,2) NOT NULL DEFAULT 0,
    precioNew DECIMAL(18,2) NOT NULL DEFAULT 0,
    activa TINYINT(1) NOT NULL DEFAULT 1,
    fecha_carga DATETIME NULL,

    PRIMARY KEY (id),
    INDEX idx_tarifasgas_empresa (empresa),
    INDEX idx_tarifasgas_tipo_cliente (tipo_cliente),
    INDEX idx_tarifasgas_peaje (peaje_gas),
    INDEX idx_tarifasgas_activa (activa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
