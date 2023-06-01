using SERVICE_MARKET.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
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
            return View();
        }

        [HttpPost]
        public ActionResult Registrar(multipleModel oUsuarios)
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
                ViewData["MENSAJE"] = "Las contraseñas no coinciden";
                return View();
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

            ViewData["MENSAJE"] = mensaje;

            if (registrado)
            {
                return RedirectToAction("Login", "USUARIOS");
            }
            else
            {
                return View();
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*CONSULTAR CIUDADES DISPONIBLES EN EL SELECT DEL FORMULARIO DE REGISTRO*/
        public ActionResult selectCiudades()
        {
            model = new List<multipleModel>();

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

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA INICIAR SESION USUARIO*/
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(multipleModel oUsuarios)
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
                        ViewData["MENSAJE"] = "Usuario no encontrado";
                    }
                }
            }
            return View();
        }


        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA SELECCIONAR ROL*/
        [Authorize]
        public ActionResult Rol()
        {
            multipleModel usuario = (multipleModel)Session["Usuario"];
            ViewBag.TipoSeleccionado = usuario.TIPO;
            return View(usuario);
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/
        
        /*METODO PARA CONSULTAR LAS PUBLICACIONES Y SOLICITUDES DE SERVICIOS DISPONIBLES*/
        [Authorize]
        public ActionResult Publicaciones_Solicitudes(string TIPO = null, int pagina = 1, int elementosPorPagina = 12)
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
            ViewBag.TipoSeleccionado = TIPO;

            return View(serviciosPagina);
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA BUSCAR LAS PUBLICACIONES Y SOLICITUDES DE SERVICIOS DISPONIBLES*/
        [Authorize]
        public ActionResult Buscar(string NOMBRE_SER, string TIPO = null, int pagina = 1, int elementosPorPagina = 12)
        {
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
            /*Calcular los índices de inicio y fin para la página actual*/
            int indiceInicio = (pagina - 1) * elementosPorPagina;
            int indiceFin = indiceInicio + elementosPorPagina;

            /*Obtener la lista de servicios para la página actual*/
            List<multipleModel> serviciosPagina = model.Skip(indiceInicio).Take(elementosPorPagina).ToList();

            ViewBag.TotalPaginas = (int)Math.Ceiling((double)model.Count / elementosPorPagina);
            ViewBag.PaginaActual = pagina;
            ViewBag.TipoSeleccionado = TIPO;

            return View(serviciosPagina);
        }
    }
}