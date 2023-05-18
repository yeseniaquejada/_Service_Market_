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
        public ActionResult Index()
        {
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
                        oServicios.PRECIO_SER =  decimal.Parse(dr["PRECIO_SER"].ToString());
                        oServicios.DESCRIPCION_BREVE = dr["DESCRIPCION_BREVE"].ToString();
                        oServicios.TIPO = dr["TIPO"].ToString();
                        oServicios.NOMBRE_CAT = dr["NOMBRE_CAT"].ToString();
                        model.Add(oServicios);

                    }
                }
                return View(model);
            }
        }
}