/*CREACION DE LA BASE DE DATOS DEL PROYECTO -- SERVICE MARKET*/
CREATE DATABASE SERVICE_MARKET
USE SERVICE_MARKET

-----------------------------------------------------------------------------------------------------------------------
/*CREACION DE LA TABLA CIUDADES*/
CREATE TABLE CIUDADES(
ID_CIUDAD INT PRIMARY KEY IDENTITY(1,1),
NOMBRE_CIUDAD VARCHAR(50) NOT NULL)

/*PROCEDIMIENTO ALMACENADO PARA CREAR REGISTROS DE LA TABLA CIUDADES*/
CREATE PROCEDURE CREAR_CIUDADES(
@NOMBRE_CIUDAD VARCHAR(50))
AS
BEGIN
	INSERT INTO CIUDADES(NOMBRE_CIUDAD) VALUES (@NOMBRE_CIUDAD)
END

/*PROCEDIMIENTO ALMACENADO PARA LEER REGISTROS DE LA TABLA CIUDADES*/
CREATE PROCEDURE LEER_CIUDADES
AS
BEGIN
	SELECT * FROM CIUDADES
END

EXEC LEER_CIUDADES

/*PROCEDIMIENTO ALMACENADO PARA ACTUALIZAR REGISTROS DE LA TABLA CIUDADES*/
CREATE PROCEDURE ACTUALIZAR_CIUDADES(
@ID_CIUDAD INT,
@NOMBRE_CIUDAD VARCHAR(50))
AS
BEGIN
	UPDATE CIUDADES SET NOMBRE_CIUDAD = @NOMBRE_CIUDAD WHERE ID_CIUDAD = @ID_CIUDAD
END

/*PROCEDIMIENTO ALMACENADO PARA BORRAR REGISTROS DE LA TABLA CIUDADES*/
CREATE PROCEDURE BORRAR_CIUDADES(
@ID_CIUDAD INT)
AS
BEGIN
	DELETE FROM CIUDADES WHERE ID_CIUDAD = @ID_CIUDAD
END

/*CREACION DE DISPARADOR QUE PERMITA INSERTAR UNA CIUDAD SIEMPRE Y CUANDO EL NOMBRE SEA UNICO*/
CREATE TRIGGER TR_CIUDADES_INSERTAR
ON CIUDADES
FOR INSERT
AS
	IF (SELECT COUNT (*) FROM INSERTED, CIUDADES WHERE INSERTED.NOMBRE_CIUDAD = CIUDADES.NOMBRE_CIUDAD) > 1
	BEGIN
	ROLLBACK TRANSACTION
	PRINT 'LA CIUDAD SE ENCUENTRA REGISTRADA'
END
	ELSE
	PRINT 'LA CIUDAD FUE INGRESADA EN LA BASE DE DATOS'
GO

/*REINICIAR CONTADOR DEL ID*/
DBCC CHECKIDENT (CIUDADES, RESEED, 0)

-----------------------------------------------------------------------------------------------------------------------

/*CREACION DE LA TABLA USUARIOS*/
CREATE TABLE USUARIOS(
ID_USUARIO INT PRIMARY KEY IDENTITY(1,1),
NOMBRE_COMPLETO_USU VARCHAR(100),
CELULAR_USU VARCHAR(20) NOT NULL,
ID_CIUDAD_FK INT REFERENCES CIUDADES(ID_CIUDAD),
CORREO_ELECTRONICO_USU VARCHAR(100) NOT NULL,
CONTRASENA_USU VARCHAR(500) NOT NULL)

/*PROCEDIMIENTO ALMACENADO PARA CREAR REGISTROS DE LA TABLA USUARIOS*/
CREATE PROCEDURE REGISTRAR_USUARIOS(
@NOMBRE_COMPLETO_USU VARCHAR(100),
@CELULAR_USU VARCHAR(20),
@ID_CIUDAD_FK INT,
@CORREO_ELECTRONICO_USU VARCHAR(100),
@CONTRASENA_USU VARCHAR(500),
@REGISTRADO BIT OUTPUT,
@MENSAJE VARCHAR(100) OUTPUT)
AS 
BEGIN
	IF(NOT EXISTS(SELECT * FROM USUARIOS WHERE CORREO_ELECTRONICO_USU = @CORREO_ELECTRONICO_USU))
	BEGIN 
		INSERT INTO USUARIOS
		VALUES (@NOMBRE_COMPLETO_USU,@CELULAR_USU,@ID_CIUDAD_FK,@CORREO_ELECTRONICO_USU,@CONTRASENA_USU)
		SET @REGISTRADO = 1
		SET @MENSAJE = 'Usuario registrado'
	END
	ELSE
	BEGIN
		SET @REGISTRADO = 0
		SET @MENSAJE = 'Correo ya existe'
	END
END

/*PROCEDIMIENTO ALMACENADO PARA LEER REGISTROS DE LA TABLA USUARIOS*/
CREATE PROCEDURE LEER_USUARIOS
AS
BEGIN
	SELECT * FROM USUARIOS
END 

EXEC LEER_USUARIOS

/*PROCEDIMIENTO ALMACENADO PARA ACTUALIZAR REGISTROS DE LA TABLA USUARIOS*/
CREATE PROCEDURE ACTUALIZAR_USUARIOS(
@ID_USUARIO INT,
@NOMBRE_COMPLETO_USU VARCHAR(100),
@CELULAR_USU VARCHAR(20),
@ID_CIUDAD_FK INT,
@CORREO_ELECTRONICO_USU VARCHAR(100),
@CONTRASENA_USU VARCHAR(500))
AS
BEGIN
	UPDATE USUARIOS 
	SET NOMBRE_COMPLETO_USU = @NOMBRE_COMPLETO_USU, CELULAR_USU = @CELULAR_USU, ID_CIUDAD_FK = @ID_CIUDAD_FK, CORREO_ELECTRONICO_USU = @CORREO_ELECTRONICO_USU, CONTRASENA_USU = @CONTRASENA_USU
	WHERE ID_USUARIO= @ID_USUARIO
END

/*PROCEDIMIENTO ALMACENADO PARA BORRAR REGISTROS DE LA TABLA USUARIOS */
CREATE PROCEDURE BORRAR_USUARIOS(
@ID_USUARIO INT)
AS
BEGIN
	DELETE FROM USUARIOS WHERE ID_USUARIO = @ID_USUARIO
END

/*PROCEDIMIENTO ALMACENADO PARA VALIDAR USUARIOS*/
CREATE PROCEDURE VALIDAR_USUARIO(
@CORREO_ELECTRONICO_USU VARCHAR(100),
@CONTRASENA_USU VARCHAR(500))
AS
BEGIN
	IF(EXISTS(SELECT * FROM USUARIOS WHERE CORREO_ELECTRONICO_USU = @CORREO_ELECTRONICO_USU AND CONTRASENA_USU = @CONTRASENA_USU))
		SELECT * FROM USUARIOS WHERE CORREO_ELECTRONICO_USU = @CORREO_ELECTRONICO_USU AND CONTRASENA_USU = @CONTRASENA_USU
	ELSE
		SELECT '0'
END

-----------------------------------------------------------------------------------------------------------------------

/*CREACION DE LA TABLA CATEGORIAS*/
CREATE TABLE CATEGORIAS(
ID_CATEGORIA INT IDENTITY(1,1) PRIMARY KEY,
NOMBRE_CAT VARCHAR(MAX) NOT NULL,
DESCRIPCION_CAT VARCHAR(MAX) NOT NULL)

/*PROCEDIMIENTO ALMACENADO PARA CREAR REGISTROS DE LA TABLA CATEGORIAS*/
CREATE PROCEDURE CREAR_CATEGORIAS(
@NOMBRE_CAT VARCHAR(MAX),
@DESCRIPCION_CAT VARCHAR(MAX))
AS
BEGIN
	INSERT INTO CATEGORIAS(NOMBRE_CAT,DESCRIPCION_CAT) VALUES (@NOMBRE_CAT,@DESCRIPCION_CAT)
END

/*PROCEDIMIENTO ALMACENADO PARA LEER REGISTROS DE LA TABLA CATEGORIAS */
CREATE PROCEDURE LEER_CATEGORIAS
AS
BEGIN
	SELECT * FROM CATEGORIAS 
END

EXEC LEER_CATEGORIAS

/*PROCEDIMIENTO ALMACENADO PARA ACTUALIZAR REGISTROS DE LA TABLA CATEGORIAS */
CREATE PROCEDURE ACTUALIZAR_CATEGORIAS(
@ID_CATEGORIA INT,
@NOMBRE_CAT VARCHAR(MAX),
@DESCRIPCION_CAT VARCHAR(MAX))
AS
BEGIN
	UPDATE CATEGORIAS SET NOMBRE_CAT = @NOMBRE_CAT, DESCRIPCION_CAT = @DESCRIPCION_CAT WHERE ID_CATEGORIA = @ID_CATEGORIA
END

/*PROCEDIMIENTO ALMACENADO PARA BORRAR REGISTROS DE LA TABLA CATEGORIAS */
CREATE PROCEDURE BORRAR_CATEGORIAS(
@ID_CATEGORIA INT)
AS
BEGIN
	DELETE FROM CATEGORIAS WHERE ID_CATEGORIA = @ID_CATEGORIA
END

/*CREACION DE DISPARADOR QUE PERMITA INSERTAR UNA CATEGORIA SIEMPRE Y CUANDO EL NOMBRE SEA UNICO*/
CREATE TRIGGER TR_CATEGORIAS_INSERTAR
ON CATEGORIAS
FOR INSERT
AS
IF (SELECT COUNT (*) FROM INSERTED, CATEGORIAS
WHERE INSERTED.NOMBRE_CAT = CATEGORIAS.NOMBRE_CAT) > 1
BEGIN
ROLLBACK TRANSACTION
PRINT 'LA CATEGORIA SE ENCUENTRA REGISTRADA'
END
ELSE
PRINT 'LA CATEGORIA FUE INGRESADA EN LA BASE DE DATOS'
GO

/*REINICIAR CONTADOR DEL ID*/
DBCC CHECKIDENT (CATEGORIAS, RESEED, 0)

-----------------------------------------------------------------------------------------------------------------------

/*CREACION DE LA TABLA ADMINISTRADORES*/
CREATE TABLE ADMINISTRADORES(
ID_ADMINISTRADOR INT PRIMARY KEY IDENTITY(1,1),
NOMBRE_COMPLETO_ADMIN VARCHAR(100),
CORREO_ELECTRONICO_ADMIN VARCHAR(100) NOT NULL,
CONTRASENA_ADMIN VARCHAR(500) NOT NULL)

 /*PROCEDIMIENTO ALMACENADO PARA CREAR REGISTROS DE LA TABLA ADMINISTRADORES*/
CREATE PROCEDURE REGISTRAR_ADMINISTRADORES(
@NOMBRE_COMPLETO_ADMIN VARCHAR(100),
@CORREO_ELECTRONICO_ADMIN VARCHAR(100),
@CONTRASENA_ADMIN VARCHAR(500),
@REGISTRADO BIT OUTPUT,
@MENSAJE VARCHAR(100) OUTPUT)
AS 
BEGIN
	IF(NOT EXISTS(SELECT * FROM ADMINISTRADORES WHERE CORREO_ELECTRONICO_ADMIN = @CORREO_ELECTRONICO_ADMIN))
	BEGIN 
		INSERT INTO ADMINISTRADORES
		VALUES (@NOMBRE_COMPLETO_ADMIN,@CORREO_ELECTRONICO_ADMIN,@CONTRASENA_ADMIN)
		SET @REGISTRADO = 1
		SET @MENSAJE = 'Administrador registrado'
	END
	ELSE
	BEGIN
		SET @REGISTRADO = 0
		SET @MENSAJE = 'Correo ya existe'
	END
END

/*PROCEDIMIENTO ALMACENADO PARA LEER REGISTROS DE LA TABLA ADMINISTRADORES*/
CREATE PROCEDURE LEER_ADMINISTRADORES
AS
BEGIN
	SELECT * FROM ADMINISTRADORES
END 

EXEC LEER_ADMINISTRADORES

/*PROCEDIMIENTO ALMACENADO PARA ACTUALIZAR REGISTROS DE LA TABLA ADMINISTRADORES*/
CREATE PROCEDURE ACTUALIZAR_ADMINISTRADORES(
@ID_ADMINISTRADOR INT,
@NOMBRE_COMPLETO_ADMIN VARCHAR(100),
@CORREO_ELECTRONICO_ADMIN VARCHAR(100),
@CONTRASENA_ADMIN VARCHAR(500))
AS
BEGIN
	UPDATE ADMINISTRADORES 
	SET NOMBRE_COMPLETO_ADMIN = @NOMBRE_COMPLETO_ADMIN, CORREO_ELECTRONICO_ADMIN = @CORREO_ELECTRONICO_ADMIN, CONTRASENA_ADMIN = @CONTRASENA_ADMIN
	WHERE ID_ADMINISTRADOR= @ID_ADMINISTRADOR
END

/*PROCEDIMIENTO ALMACENADO PARA BORRAR REGISTROS DE LA TABLA ADMINISTRADORES */
CREATE PROCEDURE BORRAR_ADMINISTRADORES(
@ID_ADMINISTRADOR INT)
AS
BEGIN
	DELETE FROM ADMINISTRADORES WHERE ID_ADMINISTRADOR = @ID_ADMINISTRADOR
END

/*PROCEDIMIENTO ALMACENADO PARA VALIDAR ADMINISTRADORES*/
CREATE PROCEDURE VALIDAR_ADMINISTRADORES(
@CORREO_ELECTRONICO_ADMIN VARCHAR(100),
@CONTRASENA_ADMIN VARCHAR(500))
AS
BEGIN
	IF(EXISTS(SELECT * FROM ADMINISTRADORES WHERE CORREO_ELECTRONICO_ADMIN = @CORREO_ELECTRONICO_ADMIN AND CONTRASENA_ADMIN = @CONTRASENA_ADMIN))
		SELECT * FROM ADMINISTRADORES WHERE CORREO_ELECTRONICO_ADMIN = @CORREO_ELECTRONICO_ADMIN AND CONTRASENA_ADMIN = @CONTRASENA_ADMIN
	ELSE
		SELECT '0'
END

-----------------------------------------------------------------------------------------------------------------------

/*CREACION DE LA TABLA SERVICIOS*/
CREATE TABLE SERVICIOS(
ID_SERVICIO INT IDENTITY(1,1) PRIMARY KEY,
NOMBRE_SER VARCHAR(70) NOT NULL,
PRECIO_SER DECIMAL NOT NULL,
DESCRIPCION_BREVE VARCHAR(500) NOT NULL,
TERMINOS_SER VARCHAR(MAX) NOT NULL,
ESTADO_DS VARCHAR(20) DEFAULT 'Activo',
TIPO VARCHAR(50) NOT NULL,
FECHA_PUBLICACION DATE NOT NULL DEFAULT GETDATE(),
ID_USUARIO_FK INT REFERENCES USUARIOS(ID_USUARIO),
ID_ADMINISTRADOR_FK INT REFERENCES ADMINISTRADORES(ID_ADMINISTRADOR),
ID_CATEGORIA_FK INT REFERENCES CATEGORIAS(ID_CATEGORIA))

/*PROCEDIMIENTO ALMACENADO PARA CREAR REGISTROS DE LA TABLA SERVICIOS*/
CREATE PROCEDURE CREAR_SERVICIOS(
@NOMBRE_SER VARCHAR(70),
@PRECIO_SER DECIMAL,
@DESCRIPCION_BREVE VARCHAR(500),
@TERMINOS_SER VARCHAR(MAX),
@TIPO VARCHAR(50),
@ID_USUARIO_FK INT,
@ID_CATEGORIA_FK INT
)
AS 
BEGIN
	INSERT INTO SERVICIOS(NOMBRE_SER,PRECIO_SER,DESCRIPCION_BREVE,TERMINOS_SER,TIPO,ID_USUARIO_FK,ID_CATEGORIA_FK) 
	VALUES (@NOMBRE_SER,@PRECIO_SER,@DESCRIPCION_BREVE,@TERMINOS_SER,@TIPO,@ID_USUARIO_FK,@ID_CATEGORIA_FK)
END

/*PROCEDIMIENTO ALMACENADO PARA LEER REGISTROS DE LA TABLA SERVICIOS */
CREATE PROCEDURE LEER_SERVICIOS
AS
BEGIN
	SELECT * FROM SERVICIOS 
END

EXEC LEER_SERVICIOS

/*PROCEDIMIENTO ALMACENADO PARA ACTUALIZAR REGISTROS DE LA TABLA SERVICIOS */
CREATE PROCEDURE ACTUALIZAR_SERVICIOS(
@ID_SERVICIO INT,
@NOMBRE_SER VARCHAR(70),
@PRECIO_SER DECIMAL,
@DESCRIPCION_BREVE VARCHAR(500),
@TERMINOS_SER VARCHAR(MAX),
@ESTADO_DS  VARCHAR(20),
@ID_CATEGORIA_FK INT)
AS
BEGIN
	UPDATE SERVICIOS SET NOMBRE_SER = @NOMBRE_SER, PRECIO_SER = @PRECIO_SER, DESCRIPCION_BREVE = @DESCRIPCION_BREVE, TERMINOS_SER = @TERMINOS_SER, ESTADO_DS = @ESTADO_DS, ID_CATEGORIA_FK = @ID_CATEGORIA_FK 
	WHERE ID_SERVICIO = @ID_SERVICIO
END

/*PROCEDIMIENTO ALMACENADO PARA BORRAR REGISTROS DE LA TABLA SERVICIOS */
CREATE PROCEDURE BORRAR_SERVICIOS(
@ID_SERVICIO INT)
AS
BEGIN
	DELETE FROM SERVICIOS WHERE ID_SERVICIO = @ID_SERVICIO
	UPDATE HISTORIAL_SERVICIOS SET ESTADO_DS = 'Desactivo' WHERE ID_SERVICIO = @ID_SERVICIO
END

/*PROCEDIMIENTO ALMACENADO PARA CONSULTAR PUBLICACIONES Y SOLICITUDES DE SERVICIOS INNER JOIN CON TABLA CATEGORIAS*/
CREATE PROCEDURE CONSULTAR_SERVICIOS
AS 
BEGIN
	SELECT ID_SERVICIO, NOMBRE_SER, PRECIO_SER, DESCRIPCION_BREVE, NOMBRE_CAT, TIPO
	FROM SERVICIOS INNER JOIN CATEGORIAS 
	ON SERVICIOS.ID_CATEGORIA_FK = CATEGORIAS.ID_CATEGORIA
END

EXEC CONSULTAR_SERVICIOS

/*REINICIAR CONTADOR DEL ID*/
DBCC CHECKIDENT (SERVICIOS, RESEED, 0)

-----------------------------------------------------------------------------------------------------------------------

/*CREACION DE LA TABLA HISTORIAL DE SERVICIOS ELIMINADOS*/
CREATE TABLE [HISTORIAL_SERVICIOS](
	[ID_SERVICIO] [int] NOT NULL,
	[NOMBRE_SER] [varchar](70) NOT NULL,
	[PRECIO_SER] [decimal] NOT NULL,
	[DESCRIPCION_BREVE] [varchar](500) NOT NULL,
	[TERMINOS_SER] [varchar](max) NOT NULL,
	[ESTADO_DS] [varchar](20) NULL,
	[TIPO] [varchar](50) NOT NULL,
	[FECHA_PUBLICACION] [date] NOT NULL,
	[ID_USUARIO_FK] [int] NULL,
	[ID_ADMINISTRADOR_FK] [int] NULL,
	[ID_CATEGORIA_FK] [int] NULL)

/*CREACION DE DISPARADOR PARA QUE AGREGUE SERVICIOS ELIMINADOS AL HISTORIAL*/
CREATE TRIGGER TR_HISTORIAL_SERVICIOS
ON SERVICIOS FOR DELETE
AS 
BEGIN 
	INSERT INTO [HISTORIAL_SERVICIOS]
	SELECT * FROM deleted
END 
GO