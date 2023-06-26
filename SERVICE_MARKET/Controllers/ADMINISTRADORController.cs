using SERVICE_MARKET.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SERVICE_MARKET.Controllers
{
    public class ADMINISTRADORController : Controller
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
        /*METODO PARA INICIAR SESION ADMINISTRADOR*/
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(multipleModel oAdmin)
        {
            try
            {

                /*ENCRIPTANDO CONTRASEÑA*/
                oAdmin.CONTRASENA_ADMIN = Encriptar(oAdmin.CONTRASENA_ADMIN);

                /*CONECTANDO BASE DE DATOS*/
                using (SqlConnection cn = new SqlConnection(conexion))
                {
                    /*PROCEDIMIENTO ALMACENADO VALIDAR ADMINISTRADOR*/
                    SqlCommand cmd = new SqlCommand("VALIDAR_ADMINISTRADOR", cn);
                    cmd.Parameters.AddWithValue("CORREO_ELECTRONICO_ADMIN", oAdmin.CORREO_ELECTRONICO_ADMIN);
                    cmd.Parameters.AddWithValue("CONTRASENA_ADMIN", oAdmin.CONTRASENA_ADMIN);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cn.Open();

                    /*LEER DATOS DEL ADMINISTRADOR*/
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        if (Convert.ToInt32(dr["ID_ADMINISTRADOR"]) > 0)
                        {
                            oAdmin.ID_ADMINISTRADOR = Convert.ToInt32(dr["ID_ADMINISTRADOR"]);
                            oAdmin.NOMBRE_COMPLETO_ADMIN = dr["NOMBRE_COMPLETO_ADMIN"].ToString();
                            /*ACCESO A VISTAS*/
                            FormsAuthentication.SetAuthCookie(oAdmin.ID_ADMINISTRADOR.ToString(), false);
                            Session["Administrador"] = oAdmin;
                            return RedirectToAction("Index", "ADMINISTRADOR");
                        }
                        else
                        {
                            ViewData["MENSAJE_VALIDACION"] = "Administrador no encontrado";
                        }
                    }
                }
            }
            catch (Exception)
            {
                ViewData["MENSAJE_VALIDACION"] = "Se produjo un error al procesar la solicitud.";
            }
            return View();
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PAGINA DE INICIO DEL ADMINISTRADOR*/
        [Authorize]
        public ActionResult Index()
        {
            int ID_ADMINISTRADOR = ObtenerIdAdministradorSesion();

            multipleModel oAdmin = (multipleModel)Session["Administrador"];
            oAdmin.ID_ADMINISTRADOR = ID_ADMINISTRADOR;

            oAdmin.NumeroCiudades = ObtenerNumeroCiudades();
            oAdmin.NumeroUsuarios = ObtenerNumeroUsuarios();
            oAdmin.NumeroCategorias = ObtenerNumeroCategorias();
            oAdmin.NumeroAdministradores = ObtenerNumeroAdministradores();
            oAdmin.NumeroServicios = ObtenerNumeroServicios();

            return View(oAdmin);
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA OBTENER EL ID DEL ADMINISTRADOR DE LA SESION*/
        private int ObtenerIdAdministradorSesion()
        {
            if (Session["Administrador"] != null)
            {
                multipleModel oAdmin = (multipleModel)Session["Administrador"];
                return oAdmin.ID_ADMINISTRADOR;
            }

            return 0;
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*FUNCIÓN PARA OBTENER EL NÚMERO DE CIUDADES REGISTRADAS*/
        private int ObtenerNumeroCiudades()
        {
            int numeroCiudades = 0;

            using (SqlConnection cn = new SqlConnection(conexion))
            {
                cn.Open();
                SqlCommand command = new SqlCommand("SELECT dbo.NUMERO_CIUDADES() AS NumeroCiudades;", cn);
                numeroCiudades = (int)command.ExecuteScalar();
            }

            return numeroCiudades;
        }

        /*FUNCIÓN PARA OBTENER EL NÚMERO DE USUARIOS REGISTRADOS*/
        private int ObtenerNumeroUsuarios()
        {
            int numeroUsuarios = 0;

            using (SqlConnection cn = new SqlConnection(conexion))
            {
                cn.Open();
                SqlCommand command = new SqlCommand("SELECT dbo.NUMERO_USUARIOS() AS NumeroUsuarios;", cn);
                numeroUsuarios = (int)command.ExecuteScalar();
            }

            return numeroUsuarios;
        }

        /*FUNCIÓN PARA OBTENER EL NÚMERO DE CATEGORÍAS REGISTRADAS*/
        private int ObtenerNumeroCategorias()
        {
            int numeroCategorias = 0;

            using (SqlConnection cn = new SqlConnection(conexion))
            {
                cn.Open();
                SqlCommand command = new SqlCommand("SELECT dbo.NUMERO_CATEGORIAS() AS NumeroCategorias;", cn);
                numeroCategorias = (int)command.ExecuteScalar();
            }

            return numeroCategorias;
        }

        /*FUNCIÓN PARA OBTENER EL NÚMERO DE ADMINISTRADORES REGISTRADOS*/
        private int ObtenerNumeroAdministradores()
        {
            int numeroAdministradores = 0;

            using (SqlConnection cn = new SqlConnection(conexion))
            {
                cn.Open();
                SqlCommand command = new SqlCommand("SELECT dbo.NUMERO_ADMINISTRADORES() AS NumeroAdministradores;", cn);
                numeroAdministradores = (int)command.ExecuteScalar();
            }

            return numeroAdministradores;
        }

        /*FUNCIÓN PARA OBTENER EL NÚMERO DE SERVICIOS REGISTRADOS*/
        private int ObtenerNumeroServicios()
        {
            int numeroServicios = 0;

            using (SqlConnection cn = new SqlConnection(conexion))
            {
                cn.Open();
                SqlCommand command = new SqlCommand("SELECT dbo.NUMERO_SERVICIOS() AS NumeroServicios;", cn);
                numeroServicios = (int)command.ExecuteScalar();
            }

            return numeroServicios;
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CONSULTAR LAS CIUDADES DISPONIBLES*/
        [Authorize]
        public ActionResult Ciudades(int pagina = 1, int elementosPorPagina = 12)
        {
            try
            {
                List<multipleModel> model = new List<multipleModel>();

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
                /* Calcular los índices de inicio y fin para la página actual */
                int indiceInicio = (pagina - 1) * elementosPorPagina;
                int indiceFin = indiceInicio + elementosPorPagina;

                /* Obtener la lista de ciudades para la página actual */
                List<multipleModel> ciudadesPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

                ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
                ViewBag.PaginaActual = pagina;

                return View(ciudadesPagina);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al consultar las ciudades. Por favor, inténtalo de nuevo más tarde.";
                return View("Ciudades");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CREAR NUEVAS CIUDADES*/
        [Authorize]
        [HttpPost]
        public ActionResult CrearCiudades(multipleModel oCiudades)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand cmd = new SqlCommand("CREAR_CIUDADES", oconexion);
                    cmd.Parameters.AddWithValue("NOMBRE_CIUDAD", oCiudades.NOMBRE_CIUDAD);
                    cmd.Parameters.Add("MENSAJE_CIUDAD", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();
                    
                    string MENSAJE_CIUDAD = cmd.Parameters["MENSAJE_CIUDAD"].Value.ToString();

                    if (!string.IsNullOrEmpty(MENSAJE_CIUDAD))
                    {
                        TempData["MessageExito"] = MENSAJE_CIUDAD;
                    }
                    else
                    {
                        TempData["MessageExito"] = "La ciudad se creó correctamente.";
                    }

                }
                return RedirectToAction("Ciudades", "ADMINISTRADOR");
            } 
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al crear la ciudad. Por favor, inténtalo de nuevo más tarde.";
                return View("Ciudades");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA BORRAR CIUDADES EXISTENTES*/
        [Authorize]
        [HttpPost]
        public ActionResult BorrarCiudad(int ID_CIUDAD)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand cmd = new SqlCommand("BORRAR_CIUDADES", oconexion);
                    cmd.Parameters.AddWithValue("ID_CIUDAD", ID_CIUDAD);
                    cmd.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Ciudades", "ADMINISTRADOR");
            } 
            catch (Exception)
            {
                return View("Ciudades");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA EDITAR CIUDADES EXISTENTES*/
        [Authorize]

        [HttpPost]
        public ActionResult EditarCiudad(int ID_CIUDAD, multipleModel oCiudades)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand cmd = new SqlCommand("ACTUALIZAR_CIUDADES", oconexion);
                    cmd.Parameters.AddWithValue("@ID_CIUDAD", ID_CIUDAD);
                    cmd.Parameters.AddWithValue("@NOMBRE_CIUDAD", oCiudades.NOMBRE_CIUDAD);
                    cmd.Parameters.Add("@MENSAJE_CIUDAD_ACT", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    string MENSAJE_CIUDAD_ACT = cmd.Parameters["@MENSAJE_CIUDAD_ACT"].Value.ToString();

                    if (!string.IsNullOrEmpty(MENSAJE_CIUDAD_ACT))
                    {
                        TempData["MessageExito"] = MENSAJE_CIUDAD_ACT;
                    }
                    else
                    {
                        TempData["MessageExito"] = "La ciudad ha sido actualizada exitosamente.";
                    }

                }
                return RedirectToAction("Ciudades", "ADMINISTRADOR");
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al actualizar la ciudad. Por favor, inténtalo de nuevo más tarde.";
                return View("Ciudades");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CONSULTAR LAS CATEGORIAS DISPONIBLES*/
        [Authorize]
        public ActionResult Categorias(int pagina = 1, int elementosPorPagina = 12)
        {
            try
            {
                List<multipleModel> model = new List<multipleModel>();

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
                /* Calcular los índices de inicio y fin para la página actual */
                int indiceInicio = (pagina - 1) * elementosPorPagina;
                int indiceFin = indiceInicio + elementosPorPagina;

                /* Obtener la lista de ciudades para la página actual */
                List<multipleModel> categoriasPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

                ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
                ViewBag.PaginaActual = pagina;

                return View(categoriasPagina);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al consultar las categorías. Por favor, inténtalo de nuevo más tarde.";
                return View("Categorias");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CREAR NUEVAS CATEGORIAS*/
        [Authorize]
        [HttpPost]
        public ActionResult CrearCategorias(multipleModel oCategorias)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand cmd = new SqlCommand("CREAR_CATEGORIAS", oconexion);
                    cmd.Parameters.AddWithValue("NOMBRE_CAT", oCategorias.NOMBRE_CAT);
                    cmd.Parameters.AddWithValue("DESCRIPCION_CAT", oCategorias.DESCRIPCION_CAT);
                    cmd.Parameters.Add("MENSAJE_CATEGORIAS", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    string MENSAJE_CATEGORIAS = cmd.Parameters["MENSAJE_CATEGORIAS"].Value.ToString();

                    if (!string.IsNullOrEmpty(MENSAJE_CATEGORIAS))
                    {
                        TempData["MessageExitoCat"] = MENSAJE_CATEGORIAS;
                    }
                    else
                    {
                        TempData["MessageExitoCat"] = "La categoría se creó correctamente.";
                    }

                }
                return RedirectToAction("Categorias", "ADMINISTRADOR");
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al crear la categoría. Por favor, inténtalo de nuevo más tarde.";
                return View("Categorias");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA BORRAR CATEGORIAS EXISTENTES*/
        [Authorize]
        [HttpPost]
        public ActionResult BorrarCategoria(int ID_CATEGORIA)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand cmd = new SqlCommand("BORRAR_CATEGORIAS", oconexion);
                    cmd.Parameters.AddWithValue("ID_CATEGORIA", ID_CATEGORIA);
                    cmd.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Categorias", "ADMINISTRADOR");
            }
            catch (Exception)
            {
                return View("Categorias");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA EDITAR CATEGORIAS EXISTENTES*/
        public ActionResult EditarCategoria(int ID_CATEGORIA, multipleModel oCategorias)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand cmd = new SqlCommand("ACTUALIZAR_CATEGORIAS", oconexion);
                    cmd.Parameters.AddWithValue("@ID_CATEGORIA", ID_CATEGORIA);
                    cmd.Parameters.AddWithValue("@NOMBRE_CAT", oCategorias.NOMBRE_CAT);
                    cmd.Parameters.AddWithValue("@DESCRIPCION_CAT", oCategorias.DESCRIPCION_CAT);
                    cmd.Parameters.Add("@MENSAJE_CATEGORIAS_ACT", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    string MENSAJE_CATEGORIAS_ACT = cmd.Parameters["@MENSAJE_CATEGORIAS_ACT"].Value.ToString();

                    if (!string.IsNullOrEmpty(MENSAJE_CATEGORIAS_ACT))
                    {
                        TempData["MessageExitoCat"] = MENSAJE_CATEGORIAS_ACT;
                    }
                    else
                    {
                        TempData["MessageExitoCat"] = "La categoría ha sido actualizada exitosamente.";
                    }

                }
                return RedirectToAction("Categorias", "ADMINISTRADOR");
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al actualizar la categoría. Por favor, inténtalo de nuevo más tarde.";
                return View("Categorias");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CONSULTAR LOS ADMINISTRADORES*/
        [Authorize]
        public ActionResult Administradores(int pagina = 1, int elementosPorPagina = 12)
        {
            try
            {
                List<multipleModel> model = new List<multipleModel>();

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("LEER_ADMINISTRADORES", oconexion);
                    Comand.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    using (SqlDataReader dr = Comand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            multipleModel oAdministradores = new multipleModel();

                            oAdministradores.ID_ADMINISTRADOR = Convert.ToInt32(dr["ID_ADMINISTRADOR"]);
                            oAdministradores.NOMBRE_COMPLETO_ADMIN = dr["NOMBRE_COMPLETO_ADMIN"].ToString();
                            oAdministradores.CORREO_ELECTRONICO_ADMIN = dr["CORREO_ELECTRONICO_ADMIN"].ToString();
                            model.Add(oAdministradores);
                        }
                    }
                }
                /* Calcular los índices de inicio y fin para la página actual */
                int indiceInicio = (pagina - 1) * elementosPorPagina;
                int indiceFin = indiceInicio + elementosPorPagina;

                /* Obtener la lista de ciudades para la página actual */
                List<multipleModel> administradoresPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

                ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
                ViewBag.PaginaActual = pagina;

                return View(administradoresPagina);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al consultar los administradores. Por favor, inténtalo de nuevo más tarde.";
                return View("Administradores");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CONSULTAR LOS USUARIOS*/
        [Authorize]
        public ActionResult Usuarios(int pagina = 1, int elementosPorPagina = 12)
        {
            try
            {
                List<multipleModel> model = new List<multipleModel>();

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("LEER_USUARIOS", oconexion);
                    Comand.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    using (SqlDataReader dr = Comand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            multipleModel oUsuarios = new multipleModel();

                            oUsuarios.ID_USUARIO = Convert.ToInt32(dr["ID_USUARIO"]);
                            oUsuarios.NOMBRE_COMPLETO_USU = dr["NOMBRE_COMPLETO_USU"].ToString();
                            oUsuarios.CELULAR_USU = dr["CELULAR_USU"].ToString();
                            oUsuarios.ID_CIUDAD_FK = Convert.ToInt32(dr["ID_CIUDAD_FK"]);
                            oUsuarios.NOMBRE_CIUDAD = dr["NOMBRE_CIUDAD"].ToString();
                            oUsuarios.CORREO_ELECTRONICO_USU = dr["CORREO_ELECTRONICO_USU"].ToString();
                            model.Add(oUsuarios);
                        }
                    }
                }
                /* Calcular los índices de inicio y fin para la página actual */
                int indiceInicio = (pagina - 1) * elementosPorPagina;
                int indiceFin = indiceInicio + elementosPorPagina;

                /* Obtener la lista de ciudades para la página actual */
                List<multipleModel> usuariosPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

                ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
                ViewBag.PaginaActual = pagina;

                return View(usuariosPagina);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al consultar los usuarios. Por favor, inténtalo de nuevo más tarde.";
                return View("Usuarios");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CERRAR SESION ADMINISTRADOR*/
        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            Session["Administrador"] = null;
            return RedirectToAction("Index", "HOME");
        }
    }
}