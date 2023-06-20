using SERVICE_MARKET.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SERVICE_MARKET.Controllers
{
    public class USUARIOSController : Controller
    {
        /*CADENA DE CONEXION*/
        private static string conexion = "Data Source=LAPTOP-RMAAM810;Initial Catalog=SERVICE_MARKET;Integrated Security=True";

        /*LISTA DE OBJETO*/
        private static List<multipleModel> model = new List<multipleModel>();

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA ENCRIPTAR CONTRASEÑA*/
        public static string Encriptar(string texto)
        {
            string result = string.Empty;
            byte[] encryted =
            System.Text.Encoding.Unicode.GetBytes(texto);
            result = Convert.ToBase64String(encryted);
            return result;
        }

        /*METODO PARA DESENCRIPTAR CONTRASEÑA*/
        public static string DesEncriptar(string texto)
        {
            string result = string.Empty;
            byte[] decryted =
            Convert.FromBase64String(texto);
            //result = 
            System.Text.Encoding.Unicode.GetString(decryted, 0, decryted.ToArray().Length);
            result = System.Text.Encoding.Unicode.GetString(decryted);
            return result;
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA REGISTRAR USUARIOS*/
        [HttpGet]
        public ActionResult Registrar()
        {
            multipleModel oUsuarios = new multipleModel();
            return View(oUsuarios);
        }

        [HttpPost]
        public ActionResult Registrar(multipleModel oUsuarios)
        {
            try
            {
                bool registrado;
                string mensaje;

                /*COMPARANDO CONTRASEÑAS*/
                if (oUsuarios.CONTRASENA_USU == oUsuarios.CONFIRMAR_CONTRASENA_U)
                {
                    /*ENCRIPTANDO CONTRASEÑA*/
                    oUsuarios.CONTRASENA_USU = Encriptar(oUsuarios.CONTRASENA_USU);
                }
                else
                {
                    ModelState.AddModelError("CONFIRMAR_CONTRASENA_U", "* Las contraseñas no coinciden");
                }

                /*VALIDAR LONGITUD DE DATOS*/
                if (oUsuarios.NOMBRE_COMPLETO_USU.Length > 100)
                {
                    ModelState.AddModelError("NOMBRE_COMPLETO_USU", "* El nombre excede la longitud permitida.");
                }

                if (oUsuarios.CONTRASENA_USU.Length > 10)
                {
                    ModelState.AddModelError("CONTRASENA_USU", "* La contraseña excede la longitud permitida. Ingresa una contraseña de máximo 10 caracteres");
                }

                /*VALIDAR NÚMERO DE CELULAR*/
                Regex regexCelular = new Regex(@"^[3-7][0-9]{9}$");
                if (!regexCelular.IsMatch(oUsuarios.CELULAR_USU))
                {
                    ModelState.AddModelError("CELULAR_USU", "* El número de celular ingresado no es válido.");
                }

                /*CONECTANDO BASE DE DATOS*/
                using (SqlConnection cn = new SqlConnection(conexion))
                {
                    /*PROCEDIMIENTO ALMACENADO REGISTRAR USUARIO*/
                    SqlCommand cmd = new SqlCommand("REGISTRAR_USUARIOS", cn);
                    cmd.Parameters.AddWithValue("NOMBRE_COMPLETO_USU", oUsuarios.NOMBRE_COMPLETO_USU);
                    cmd.Parameters.AddWithValue("CELULAR_USU", "57" + oUsuarios.CELULAR_USU);
                    cmd.Parameters.AddWithValue("ID_CIUDAD_FK", oUsuarios.ID_CIUDAD_FK);
                    cmd.Parameters.AddWithValue("CORREO_ELECTRONICO_USU", oUsuarios.CORREO_ELECTRONICO_USU);
                    cmd.Parameters.AddWithValue("CONTRASENA_USU", oUsuarios.CONTRASENA_USU);
                    cmd.Parameters.Add("REGISTRADO", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("MENSAJE", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cn.Open();
                    cmd.ExecuteNonQuery();

                    /*LEER PARAMETROS DE SALIDA*/
                    registrado = Convert.ToBoolean(cmd.Parameters["REGISTRADO"].Value);
                    mensaje = cmd.Parameters["MENSAJE"].Value.ToString();

                }

                ModelState.AddModelError("MENSAJE", mensaje);

                if (registrado)
                {
                    TempData["Mensaje"] = mensaje;
                    return RedirectToAction("Login", "USUARIOS");
                }
                else
                {
                    return View(oUsuarios);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "No es posible registrar usuarios en este momento. Por favor, inténtelo de nuevo más tarde.";
                return View("Registrar");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*CONSULTAR CIUDADES DISPONIBLES EN EL SELECT DEL FORMULARIO DE REGISTRO*/
        public ActionResult selectCiudades()
        {
            model = new List<multipleModel>();

            try
            {

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("LEER_CIUDADES", oconexion);
                    Comand.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    using (SqlDataReader dr = Comand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            multipleModel oCiudades = new multipleModel();
                            oCiudades.ID_CIUDAD = Convert.ToInt32(dr["ID_CIUDAD"]);
                            oCiudades.NOMBRE_CIUDAD = dr["NOMBRE_CIUDAD"].ToString();
                            model.Add(oCiudades);

                        }
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar las ciudades.";
                return View(new List<multipleModel>());
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA INICIAR SESION USUARIO*/
        [HttpGet]
        public ActionResult Login()
        {
            string mensaje = TempData["Mensaje"] as string;
            ViewBag.Mensaje = mensaje;
            return View();
        }

        [HttpPost]
        public ActionResult Login(multipleModel oUsuarios)
        {
            try
            {

                /*ENCRIPTANDO CONTRASEÑA*/
                oUsuarios.CONTRASENA_USU = Encriptar(oUsuarios.CONTRASENA_USU);

                /*CONECTANDO BASE DE DATOS*/
                using (SqlConnection cn = new SqlConnection(conexion))
                {
                    /*PROCEDIMIENTO ALMACENADO VALIDAR USUARIO*/
                    SqlCommand cmd = new SqlCommand("VALIDAR_USUARIO", cn);
                    cmd.Parameters.AddWithValue("CORREO_ELECTRONICO_USU", oUsuarios.CORREO_ELECTRONICO_USU);
                    cmd.Parameters.AddWithValue("CONTRASENA_USU", oUsuarios.CONTRASENA_USU);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cn.Open();

                    /*LEER DATOS DEL USUARIO*/
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        if (Convert.ToInt32(dr["ID_USUARIO"]) > 0)
                        {
                            oUsuarios.ID_USUARIO = Convert.ToInt32(dr["ID_USUARIO"]);
                            oUsuarios.NOMBRE_COMPLETO_USU = dr["NOMBRE_COMPLETO_USU"].ToString();
                            oUsuarios.CELULAR_USU = dr["CELULAR_USU"].ToString();
                            oUsuarios.ID_CIUDAD_FK = Convert.ToInt32(dr["ID_CIUDAD_FK"]);

                            /*ACCESO A VISTAS*/
                            FormsAuthentication.SetAuthCookie(oUsuarios.ID_USUARIO.ToString(), false);
                            Session["Usuario"] = oUsuarios;
                            return RedirectToAction("Rol", "USUARIOS");
                        }
                        else
                        {
                            ViewData["MENSAJE_VALIDACION"] = "Usuario no encontrado";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["MENSAJE_VALIDACION"] = "Se produjo un error al procesar la solicitud.";
            }
            return View();
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA SELECCIONAR ROL*/
        [Authorize]
        public ActionResult Rol()
        {
            multipleModel oUsuarios = (multipleModel)Session["Usuario"];
            string Tipo = Request.Params["TIPO"];
            oUsuarios.TIPO = Tipo;
            return View(oUsuarios);
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CONSULTAR LAS PUBLICACIONES Y SOLICITUDES DE SERVICIOS DISPONIBLES*/
        [Authorize]
        public ActionResult Publicaciones_Solicitudes(string TIPO = null, int pagina = 1, int elementosPorPagina = 12)
        {
            try
            {
                List<multipleModel> model = new List<multipleModel>();

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("CONSULTAR_PUBLICACIONES_SOLICITUDES", oconexion);
                    Comand.Parameters.Add("@TIPO", SqlDbType.VarChar);
                    Comand.Parameters["@TIPO"].Value = TIPO;
                    Comand.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    using (SqlDataReader dr = Comand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            multipleModel oServicios = new multipleModel();
                            oServicios.ID_SERVICIO = Convert.ToInt32(dr["ID_SERVICIO"]);
                            oServicios.NOMBRE_SER = dr["NOMBRE_SER"].ToString();
                            oServicios.PRECIO_SER = decimal.Parse(dr["PRECIO_SER"].ToString());
                            oServicios.DESCRIPCION_BREVE = dr["DESCRIPCION_BREVE"].ToString();
                            oServicios.TIPO = dr["TIPO"].ToString();
                            oServicios.NOMBRE_CAT = dr["NOMBRE_CAT"].ToString();
                            model.Add(oServicios);

                        }
                    }
                }
                /*Calcular los índices de inicio y fin para la página actual*/
                int indiceInicio = (pagina - 1) * elementosPorPagina;
                int indiceFin = indiceInicio + elementosPorPagina;

                /*Obtener la lista de servicios para la página actual*/
                List<multipleModel> serviciosPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

                ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
                ViewBag.PaginaActual = pagina;
                string Tipo = Request.Params["TIPO"];

                return View(serviciosPagina);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Se produjo un error al consultar los servicios disponibles. Por favor, inténtalo de nuevo más tarde.";
                return View("Publicaciones_Solicitudes");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA BUSCAR LAS PUBLICACIONES Y SOLICITUDES DE SERVICIOS DISPONIBLES*/
        [Authorize]
        public ActionResult Buscar(string NOMBRE_SER, string TIPO = null, int pagina = 1, int elementosPorPagina = 12)
        {
            string Tipo = Request.Params["TIPO"];

            try
            {
                if (string.IsNullOrEmpty(NOMBRE_SER) || string.IsNullOrEmpty(Tipo))
                {
                    ViewBag.Mensaje = "Por favor, ingresa al menos un criterio de búsqueda.";
                    return View();
                }

                List<multipleModel> model = new List<multipleModel>();

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("BUSQUEDA_COMPLETA", oconexion);
                    Comand.Parameters.Add("@NOMBRE_SER", SqlDbType.VarChar);
                    Comand.Parameters["@NOMBRE_SER"].Value = '%' + NOMBRE_SER + '%';
                    Comand.Parameters.Add("@TIPO", SqlDbType.VarChar);
                    Comand.Parameters["@TIPO"].Value = TIPO;
                    Comand.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    using (SqlDataReader dr = Comand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            multipleModel oServicios = new multipleModel();
                            oServicios.ID_SERVICIO = Convert.ToInt32(dr["ID_SERVICIO"]);
                            oServicios.NOMBRE_SER = dr["NOMBRE_SER"].ToString();
                            oServicios.PRECIO_SER = decimal.Parse(dr["PRECIO_SER"].ToString());
                            oServicios.DESCRIPCION_BREVE = dr["DESCRIPCION_BREVE"].ToString();
                            oServicios.TIPO = dr["TIPO"].ToString();
                            oServicios.NOMBRE_CAT = dr["NOMBRE_CAT"].ToString();
                            model.Add(oServicios);
                        }
                    }
                }

                if (model.Count == 0)
                {
                    ViewBag.Mensaje = "No se encontró el servicio que estabas buscando.";
                    return View();
                }

                /*Calcular los índices de inicio y fin para la página actual*/
                int indiceInicio = (pagina - 1) * elementosPorPagina;
                int indiceFin = indiceInicio + elementosPorPagina;

                /*Obtener la lista de servicios para la página actual*/
                List<multipleModel> serviciosPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

                ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
                ViewBag.PaginaActual = pagina;

                return View(serviciosPagina);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al buscar los servicios. Por favor, intenta nuevamente más tarde.";
                return View("Buscar");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CATEGORIZAR LAS PUBLICACIONES Y SOLICITUDES DE SERVICIOS DISPONIBLES*/
        [Authorize]
        public ActionResult Categorias(string TIPO = null, int? ID_CATEGORIA = null, int pagina = 1, int elementosPorPagina = 12)
        {
            try
            {
                List<multipleModel> model = new List<multipleModel>();

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("CATEGORIAS_SERVICIOS", oconexion);
                    Comand.Parameters.AddWithValue("@ID_CATEGORIA", SqlDbType.Int);
                    Comand.Parameters["@ID_CATEGORIA"].Value = ID_CATEGORIA;
                    Comand.Parameters.Add("@TIPO", SqlDbType.VarChar);
                    Comand.Parameters["@TIPO"].Value = TIPO;
                    Comand.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    using (SqlDataReader dr = Comand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            multipleModel oServicios = new multipleModel();
                            oServicios.ID_SERVICIO = Convert.ToInt32(dr["ID_SERVICIO"]);
                            oServicios.NOMBRE_SER = dr["NOMBRE_SER"].ToString();
                            oServicios.PRECIO_SER = decimal.Parse(dr["PRECIO_SER"].ToString());
                            oServicios.DESCRIPCION_BREVE = dr["DESCRIPCION_BREVE"].ToString();
                            oServicios.TIPO = dr["TIPO"].ToString();
                            oServicios.NOMBRE_CAT = dr["NOMBRE_CAT"].ToString();
                            model.Add(oServicios);

                        }
                    }
                }

                if (model.Count == 0)
                {
                    ViewBag.Mensaje = "No se encontró servicios disponibles en esta categoría.";
                    return View();
                }

                /*Calcular los índices de inicio y fin para la página actual*/
                int indiceInicio = (pagina - 1) * elementosPorPagina;
                int indiceFin = indiceInicio + elementosPorPagina;

                /*Obtener la lista de servicios para la página actual*/
                List<multipleModel> serviciosPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

                ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
                ViewBag.PaginaActual = pagina;
                string Tipo = Request.Params["TIPO"];

                return View(serviciosPagina);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Se produjo un error al buscar servicios en esta categoría. Por favor, inténtalo nuevamente más tarde.";
                return View("Buscar");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*VISTA PARA VISUALIZAR INFORMACION DE LAS CATEGORIAS*/
        [Authorize]
        public ActionResult InfoCategorias(string TIPO, multipleModel oServicios)
        {
            string Tipo = Request.Params["TIPO"];
            return View();
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CREAR NUEVOS SERVICIOS*/
        [Authorize]
        [HttpGet]
        public ActionResult CrearServicio(string TIPO)
        {
            multipleModel oServicios = new multipleModel();
            string Tipo = Request.Params["TIPO"];
            return View(oServicios);
        }

        [HttpPost]
        public ActionResult CrearServicio(multipleModel oServicios, string TIPO)
        {
            string Tipo = Request.Params["TIPO"];

            int idUsuario = ObtenerIdUsuarioSesion();

            if (idUsuario != 0)
            {

                try
                {
                    if (oServicios.NOMBRE_SER.Length > 70)
                    {
                        ModelState.AddModelError("NOMBRE_SER", "* El nombre del servicio excede la longitud permitida.");
                    }

                    if (oServicios.DESCRIPCION_BREVE.Length > 500)
                    {
                        ModelState.AddModelError("DESCRIPCION_BREVE", "* La descripción excede la longitud permitida.");
                    }

                    if (ModelState.IsValid)
                    {
                        /*CONECTANDO BASE DE DATOS*/
                        using (SqlConnection cn = new SqlConnection(conexion))
                        {

                            /*PROCEDIMIENTO ALMACENADO CREAR SERVICIOS*/
                            cn.Open();
                            SqlCommand cmd = new SqlCommand("CREAR_SERVICIOS", cn);
                            cmd.Parameters.AddWithValue("@NOMBRE_SER", oServicios.NOMBRE_SER);
                            cmd.Parameters.AddWithValue("@PRECIO_SER", oServicios.PRECIO_SER);
                            cmd.Parameters.AddWithValue("@DESCRIPCION_BREVE", oServicios.DESCRIPCION_BREVE);
                            cmd.Parameters.AddWithValue("@TERMINOS_SER", oServicios.TERMINOS_SER);
                            if (TIPO == "Publicacion")
                            {
                                cmd.Parameters.AddWithValue("@TIPO", "Solicitud");
                            }
                            else if (TIPO == "Solicitud")
                            {
                                cmd.Parameters.AddWithValue("@TIPO", "Publicacion");
                            }
                            cmd.Parameters.AddWithValue("@ID_USUARIO_FK", idUsuario);
                            cmd.Parameters.AddWithValue("@ID_CATEGORIA_FK", oServicios.ID_CATEGORIA_FK);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.ExecuteNonQuery();

                            return RedirectToAction("ServiciosUsuario", "USUARIOS", new { TIPO = TIPO });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "No es posible registrar nuevos servicios en este momento. Por favor, inténtelo de nuevo más tarde.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Debe iniciar sesión para crear un servicio.";
            }
            return View("CrearServicio", oServicios);
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA OBTENER EL ID DEL USUARIO DE LA SESION*/
        private int ObtenerIdUsuarioSesion()
        {
            if (Session["Usuario"] != null)
            {
                multipleModel oUsuarios = (multipleModel)Session["Usuario"];
                return oUsuarios.ID_USUARIO;
            }

            return 0;
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*CONSULTAR CATEGORIAS DISPONIBLES EN EL SELECT DEL FORMULARIO DE CREAR SERVICIOS*/
        public ActionResult selectCategorias()
        {
            model = new List<multipleModel>();

            try
            {

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("LEER_CATEGORIAS", oconexion);
                    Comand.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    using (SqlDataReader dr = Comand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            multipleModel oCategorias = new multipleModel();
                            oCategorias.ID_CATEGORIA = Convert.ToInt32(dr["ID_CATEGORIA"]);
                            oCategorias.NOMBRE_CAT = dr["NOMBRE_CAT"].ToString();
                            oCategorias.DESCRIPCION_CAT = dr["DESCRIPCION_CAT"].ToString();
                            model.Add(oCategorias);

                        }
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar las categorías.";
                return View(new List<multipleModel>());
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CONSULTAR MAS INFORMACION SOBRE UN SERVICIO*/
        [Authorize]
        public ActionResult InfoServicio(int ID_SERVICIO, string TIPO)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("INFORMACION_SERVICIO", oconexion);
                    Comand.Parameters.AddWithValue("@ID_SERVICIO", ID_SERVICIO);
                    Comand.Parameters.AddWithValue("@TIPO", TIPO);
                    Comand.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    List<multipleModel> informacion = new List<multipleModel>();

                    using (SqlDataReader dr = Comand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            multipleModel informacionS = new multipleModel();
                            informacionS.ID_USUARIO = Convert.ToInt32(dr["ID_USUARIO"]);
                            informacionS.NOMBRE_COMPLETO_USU = dr["NOMBRE_COMPLETO_USU"].ToString();
                            informacionS.CELULAR_USU = dr["CELULAR_USU"].ToString();
                            informacionS.NOMBRE_CIUDAD = dr["NOMBRE_CIUDAD"].ToString();
                            informacionS.ID_SERVICIO = Convert.ToInt32(dr["ID_SERVICIO"]);
                            informacionS.NOMBRE_SER = dr["NOMBRE_SER"].ToString();
                            informacionS.PRECIO_SER = decimal.Parse(dr["PRECIO_SER"].ToString());
                            informacionS.DESCRIPCION_BREVE = dr["DESCRIPCION_BREVE"].ToString();
                            informacionS.TERMINOS_SER = dr["TERMINOS_SER"].ToString();
                            informacionS.ESTADO_DS = dr["ESTADO_DS"].ToString();
                            informacionS.TIPO = dr["TIPO"].ToString();
                            informacionS.FECHA_PUBLICACION = dr["FECHA_PUBLICACION"].ToString();
                            informacionS.NOMBRE_CAT = dr["NOMBRE_CAT"].ToString();

                            informacion.Add(informacionS);
                        }
                    }

                    string referringActionName = Request.Headers["Referer"].ToString();
                    string Tipo = Request.Params["TIPO"];

                    if (referringActionName.Contains("Publicaciones_Solicitudes") 
                        || referringActionName.Contains("Categorias")
                        || referringActionName.Contains("Buscar"))
                    {
                        Tipo = Request.Params["TIPO"];
                        ViewBag.AccionActual = "PaginaPrincipal";
                    }
                    else if (referringActionName.Contains("EditarServicio") 
                        || referringActionName.Contains("ServiciosUsuario"))
                    {
                        if (Tipo == "Publicacion")
                        {
                            Tipo = "Solicitud";
                        }
                        else if (Tipo == "Solicitud")
                        {
                            Tipo = "Publicacion";
                        }

                        ViewBag.AccionActual = "PaginaUsuario";
                    }             

                    ViewBag.Tipo = Tipo;
                    return View(informacion);

                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Se produjo un error al consultar información del servicio. Por favor, inténtalo de nuevo más tarde.";
                return View("InfoServicio");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CONSULTAR SERVICIOS PUBLICADOS POR UN USUARIO*/
        [Authorize]
        public ActionResult ServiciosUsuario(string TIPO, int pagina = 1, int elementosPorPagina = 12)
        {
            string Tipo = Request.Params["TIPO"];

            int ID_USUARIO = ObtenerIdUsuarioSesion();

            if (ID_USUARIO != 0)
            {
                try
                {
                    List<multipleModel> model = new List<multipleModel>();

                    using (SqlConnection oconexion = new SqlConnection(conexion))
                    {
                        SqlCommand Comand = new SqlCommand("SERVICIOS_USUARIO", oconexion);
                        Comand.Parameters.AddWithValue("@ID_USUARIO", ID_USUARIO);
                        if (TIPO == "Publicacion")
                        {
                            Comand.Parameters.AddWithValue("@TIPO", "Solicitud");
                        }
                        else if (TIPO == "Solicitud")
                        {
                            Comand.Parameters.AddWithValue("@TIPO", "Publicacion");
                        }
                        Comand.CommandType = CommandType.StoredProcedure;
                        oconexion.Open();

                        using (SqlDataReader dr = Comand.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                multipleModel oServicios = new multipleModel();
                                oServicios.ID_SERVICIO = Convert.ToInt32(dr["ID_SERVICIO"]);
                                oServicios.NOMBRE_SER = dr["NOMBRE_SER"].ToString();
                                oServicios.PRECIO_SER = decimal.Parse(dr["PRECIO_SER"].ToString());
                                oServicios.DESCRIPCION_BREVE = dr["DESCRIPCION_BREVE"].ToString();
                                oServicios.TIPO = dr["TIPO"].ToString();
                                oServicios.NOMBRE_CAT = dr["NOMBRE_CAT"].ToString();
                                model.Add(oServicios);
                            }
                        }
                    }
                    /*Calcular los índices de inicio y fin para la página actual*/
                    int indiceInicio = (pagina - 1) * elementosPorPagina;
                    int indiceFin = indiceInicio + elementosPorPagina;

                    /*Obtener la lista de servicios para la página actual*/
                    List<multipleModel> serviciosPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

                    bool tieneServicios = serviciosPagina.Count > 0;
                    ViewBag.TieneServicios = tieneServicios;

                    ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
                    ViewBag.PaginaActual = pagina;

                    return View(serviciosPagina);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Ocurrió un error al consultar los servicios.Por favor, inténtalo de nuevo más tarde.";
                    ViewBag.TieneServicios = false;
                    return View("ServiciosUsuario");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Debe iniciar sesión para consultas tus servicios.";
            }
            ViewBag.TieneServicios = false;
            return View("ServiciosUsuario");
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA ACTUALIZAR SERVICIOS PUBLICADOS POR UN USUARIO*/
        [Authorize]
        [HttpGet]
        public ActionResult EditarServicio(int? ID_SERVICIO, multipleModel oServicios)
        {
            string Tipo = Request.Params["TIPO"];

            if (ID_SERVICIO == null)
            {
                if (oServicios.TIPO == "Publicacion")
                {
                    Tipo = "Solicitud";
                }
                else if (oServicios.TIPO == "Solicitud")
                {
                    Tipo = "Publicacion";
                }

                return RedirectToAction("Publicaciones_Solicitudes", "USUARIOS", new { TIPO = Tipo });
            }

            oServicios = ObtenerServicioPorId(ID_SERVICIO.Value);

            if (oServicios != null)
            {
                int ID_USUARIO = ObtenerIdUsuarioSesion();

                List<multipleModel> oCategorias = ObtenerCategorias();

                if (ID_USUARIO != 0 && ID_USUARIO == oServicios.ID_USUARIO_FK)
                {
                    ViewBag.Categorias = oCategorias;
                    return View(oServicios);
                }
                else
                {
                    ViewBag.ErrorMessage = "No tiene permisos para editar este servicio.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "El servicio no existe.";
            }

            string TIPO = (oServicios != null && oServicios.TIPO == "Publicacion") ? "Publicacion" : "Solicitud";
            return RedirectToAction("Publicaciones_Solicitudes", "USUARIOS", new { TIPO = TIPO });
        }

        [HttpPost]
        public ActionResult EditarServicio(multipleModel oServicios, String TIPO)
        {
            int ID_USUARIO = ObtenerIdUsuarioSesion();

            if (ID_USUARIO != 0 && ID_USUARIO == oServicios.ID_USUARIO_FK)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        using (SqlConnection cn = new SqlConnection(conexion))
                        {
                            cn.Open();
                            SqlCommand cmd = new SqlCommand("ACTUALIZAR_SERVICIO", cn);
                            cmd.Parameters.AddWithValue("@ID_SERVICIO", oServicios.ID_SERVICIO);
                            cmd.Parameters.AddWithValue("@NOMBRE_SER", oServicios.NOMBRE_SER);
                            cmd.Parameters.AddWithValue("@PRECIO_SER", oServicios.PRECIO_SER);
                            cmd.Parameters.AddWithValue("@DESCRIPCION_BREVE", oServicios.DESCRIPCION_BREVE);
                            cmd.Parameters.AddWithValue("@TERMINOS_SER", oServicios.TERMINOS_SER);
                            cmd.Parameters.AddWithValue("@ID_CATEGORIA_FK", oServicios.ID_CATEGORIA_FK);
                            cmd.Parameters.AddWithValue("@TIPO", TIPO);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.ExecuteNonQuery();

                            return RedirectToAction("InfoServicio", "USUARIOS", new { ID_SERVICIO = oServicios.ID_SERVICIO, TIPO = oServicios.TIPO });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "No es posible editar el servicio en este momento. Por favor, inténtelo de nuevo más tarde.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "No tiene permisos para editar este servicio.";
            }

            return View(oServicios);
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA OBTENER UN SERVICIO POR SU ID*/
        private multipleModel ObtenerServicioPorId(int ID_SERVICIO)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("SELECT s.*, c.NOMBRE_CAT FROM SERVICIOS s INNER JOIN CATEGORIAS c ON s.ID_CATEGORIA_FK = c.ID_CATEGORIA WHERE s.ID_SERVICIO = @ID_SERVICIO", cn);
                cmd.Parameters.AddWithValue("@ID_SERVICIO", ID_SERVICIO);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    multipleModel oServicios = new multipleModel();
                    oServicios.ID_SERVICIO = (int)reader["ID_SERVICIO"];
                    oServicios.NOMBRE_SER = reader["NOMBRE_SER"].ToString();
                    oServicios.PRECIO_SER = (decimal)reader["PRECIO_SER"];
                    oServicios.DESCRIPCION_BREVE = reader["DESCRIPCION_BREVE"].ToString();
                    oServicios.TERMINOS_SER = reader["TERMINOS_SER"].ToString();
                    oServicios.ID_CATEGORIA_FK = (int)reader["ID_CATEGORIA_FK"];
                    oServicios.ID_USUARIO_FK = (int)reader["ID_USUARIO_FK"];
                    oServicios.TIPO = reader["TIPO"].ToString();
                    oServicios.NOMBRE_CAT = reader["NOMBRE_CAT"].ToString();

                    return oServicios;
                }
            }

            return null;
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA OBTENER LISTA DE CATEGORIAS*/
        private List<multipleModel> ObtenerCategorias()
        {
            List<multipleModel> oCategorias = new List<multipleModel>();

            using (SqlConnection cn = new SqlConnection(conexion))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM CATEGORIAS", cn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    multipleModel categoria = new multipleModel();
                    categoria.ID_CATEGORIA = (int)reader["ID_CATEGORIA"];
                    categoria.NOMBRE_CAT = reader["NOMBRE_CAT"].ToString();
                    categoria.DESCRIPCION_CAT = reader["DESCRIPCION_CAT"].ToString();

                    oCategorias.Add(categoria);
                }
            }

            return oCategorias;
        }
        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA ELIMINAR SERVICIOS PUBLICADOS POR UN USUARIO*/
        [Authorize]
        public ActionResult EliminarServicio(int ID_SERVICIO)
        {
            string Tipo = Request.Params["TIPO"];

            try
            {
                int ID_USUARIO = ObtenerIdUsuarioSesion();

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("BORRAR_SERVICIOS", oconexion);
                    Comand.Parameters.AddWithValue("@ID_SERVICIO", ID_SERVICIO);
                    Comand.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();
                    Comand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                ViewBag.MensajeEliminar = "Ocurrió un error al eliminar el servicio. Por favor, inténtalo de nuevo más tarde.";
            }

            return RedirectToAction("ServiciosUsuario", "USUARIOS", new { TIPO = Tipo });
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CONSULTAR EL HISTORIAL DE SERVICIOS PUBLICADOS POR UN USUARIO*/
        [Authorize]
        public ActionResult HistorialUsuario(string TIPO, int pagina = 1, int elementosPorPagina = 12)
        {

            int ID_USUARIO = ObtenerIdUsuarioSesion();

            if (ID_USUARIO != 0)
            {
                try
                {
                    List<multipleModel> model = new List<multipleModel>();

                    using (SqlConnection oconexion = new SqlConnection(conexion))
                    {
                        SqlCommand Comand = new SqlCommand("HISTORIAL_USUARIO", oconexion);
                        Comand.Parameters.AddWithValue("@ID_USUARIO", ID_USUARIO);
                        if (TIPO == "Publicacion")
                        {
                            Comand.Parameters.AddWithValue("@TIPO", "Solicitud");
                        }
                        else if (TIPO == "Solicitud")
                        {
                            Comand.Parameters.AddWithValue("@TIPO", "Publicacion");
                        }
                        Comand.CommandType = CommandType.StoredProcedure;
                        oconexion.Open();

                        using (SqlDataReader dr = Comand.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                multipleModel oServicios = new multipleModel();
                                oServicios.ID_SERVICIO = Convert.ToInt32(dr["ID_SERVICIO"]);
                                oServicios.NOMBRE_SER = dr["NOMBRE_SER"].ToString();
                                oServicios.PRECIO_SER = decimal.Parse(dr["PRECIO_SER"].ToString());
                                oServicios.DESCRIPCION_BREVE = dr["DESCRIPCION_BREVE"].ToString();
                                oServicios.TIPO = dr["TIPO"].ToString();
                                oServicios.ESTADO_DS = dr["ESTADO_DS"].ToString();
                                oServicios.NOMBRE_CAT = dr["NOMBRE_CAT"].ToString();
                                model.Add(oServicios);
                            }
                        }
                    }

                    /*Calcular los índices de inicio y fin para la página actual*/
                    int indiceInicio = (pagina - 1) * elementosPorPagina;
                    int indiceFin = indiceInicio + elementosPorPagina;

                    /*Obtener la lista de servicios para la página actual*/
                    List<multipleModel> serviciosPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

                    bool tieneServicios = serviciosPagina.Count > 0;
                    ViewBag.TieneServiciosH = tieneServicios;

                    ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
                    ViewBag.PaginaActual = pagina;

                    return View(serviciosPagina);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Ocurrió un error al consultar tu historial de servicios. Por favor, inténtalo de nuevo más tarde.";
                    ViewBag.TieneServiciosH = false;
                    return View("HistorialUsuario");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Debe iniciar sesión para consultar tu historial de servicios.";
            }

            ViewBag.TieneServiciosH = false;
            return View("HistorialUsuario");
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CERRAR SESION USUARIOS*/
        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            Session["Usuario"] = null;
            return RedirectToAction("Index", "HOME");
        }

    }
}