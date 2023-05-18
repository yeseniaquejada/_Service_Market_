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

                /*LEER IDENTIFICACION DEL USUARIO (PRIMERA FILA)*/
                oUsuarios.ID_USUARIO = Convert.ToInt32(cmd.ExecuteScalar());
            }

            /*ACCESO A VISTAS*/
            if (oUsuarios.ID_USUARIO != 0)
            {
                FormsAuthentication.SetAuthCookie(oUsuarios.ID_USUARIO.ToString(), false);
                Session["Usuario"] = oUsuarios;
                return RedirectToAction("IndexUsuarios", "HOME");
            }
            else
            {
                ViewData["MENSAJE"] = "Usuario no encontrado";
            }
            return View();
        }

    }
}