UPDATE usuarios
SET rol = 'Proveedor'
WHERE idusuarios > 0
	AND rol = 'Comercializadora';