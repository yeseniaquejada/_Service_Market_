using SERVICE_MARKET.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SERVICE_MARKET.Controllers
{
    public class HOMEController : Controller
    {
        /*CADENA DE CONEXION*/
        private static string conexion = "Data Source=LAPTOP-RMAAM810;Initial Catalog=SERVICE_MARKET;Integrated Security=True";

        /*LISTA DE OBJETO*/
        private static List<multipleModel> model = new List<multipleModel>();

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CONSULTAR PUBLICACIONES Y SOLICITUDES DE SERVICIOS*/
        public ActionResult Index(int pagina = 1, int elementosPorPagina = 12)
        {
            try {
                List<multipleModel> model = new List<multipleModel>();

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("CONSULTAR_SERVICIOS", oconexion);
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

                return View(serviciosPagina);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Se produjo un error al consultar las publicaciones y solicitudes de servicios.";
                return View("Index");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA BUSCAR PUBLICACIONES Y SOLICITUDES DE SERVICIOS DISPONIBLES*/
        public ActionResult Buscar(string NOMBRE_SER, string TIPO = null, int pagina = 1, int elementosPorPagina = 12)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NOMBRE_SER) && string.IsNullOrWhiteSpace(TIPO))
                {
                    ViewBag.Mensaje = "Por favor, ingresa algún criterio de búsqueda.";
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
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al buscar los servicios. Por favor, intenta nuevamente más tarde.";
                return View("Buscar");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*METODO PARA CATEGORIZAR LOS SERVICIOS DISPONIBLES*/
        public ActionResult Categorias(string TIPO = "Ambos", int? ID_CATEGORIA = null, int pagina = 1, int elementosPorPagina = 12)
        {
            try {
                List<multipleModel> model = new List<multipleModel>();

                using (SqlConnection oconexion = new SqlConnection(conexion))
                {
                    SqlCommand Comand = new SqlCommand("CATEGORIAS_SERVICIOS", oconexion);
                    Comand.Parameters.AddWithValue("@ID_CATEGORIA", SqlDbType.Int);
                    Comand.Parameters["@ID_CATEGORIA"].Value = ID_CATEGORIA;
                    Comand.Parameters.AddWithValue("@TIPO", TIPO);
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

                return View(serviciosPagina);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Se produjo un error al buscar servicios en esta categoría. Por favor, inténtalo nuevamente más tarde.";
                return View("Buscar");
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*VISTA PARA VISUALIZAR INFORMACION DE LAS CATEGORIAS*/
        public ActionResult InfoCategorias()
        {
            return View();
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*VISTA PARA VISUALIZAR TERMINOS Y CONDICIONES*/
        public ActionResult TerminosCondiciones()
        {
            return View();
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*VISTA PARA VISUALIZAR NUESTROS SERVICIOS*/
        public ActionResult NuestrosServicios()
        {
            return View();
        }

        /*-----------------------------------------------------------------------------------------------------------------------*/

        /*VISTA PARA VISUALIZAR SOBRE NOSOTROS*/
        public ActionResult SobreNosotros()
        {
            return View();
        }
    }
}