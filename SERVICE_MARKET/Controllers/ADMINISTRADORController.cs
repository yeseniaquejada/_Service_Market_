﻿using SERVICE_MARKET.Models;
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
    }
}