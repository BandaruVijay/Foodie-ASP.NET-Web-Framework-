using Foodie.Admin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Foodie.User
{
    public partial class Menu : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                getCategories();
                getProducts();
            }
        }
        private void getCategories()
        {
            con = new SqlConnection(Connection.GetConnectionString());
            cmd = new SqlCommand("Category_Crud", con);
            cmd.Parameters.AddWithValue("Action", "ACTIVECAT");
            cmd.CommandType = CommandType.StoredProcedure;
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            rCategory.DataSource = dt; // or GridView/DropDownList
            rCategory.DataBind();
        }


        private void getProducts()
        {
            con = new SqlConnection(Connection.GetConnectionString());
            cmd = new SqlCommand("Product_Crud", con);
            cmd.Parameters.AddWithValue("Action", "ACTIVEPROD");
            cmd.CommandType = CommandType.StoredProcedure;
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            rProducts.DataSource = dt;
            rProducts.DataBind();
        }

        protected void rProducts_ItemCommand(object sender, CommandEventArgs e)
        {
            if (Session["userId"] != null)
            {
                bool isCartItemUpdated = false;
                int i = isItemExistInCart(Convert.ToInt32(e.CommandArgument));
                if (i == 0) 
                {
                    //Adding New Item to Cart
                    con = new SqlConnection(Connection.GetConnectionString());
                    cmd = new SqlCommand("Cart_Crud", con);
                    cmd.Parameters.AddWithValue("@Action", "ACTIVEPROD");
                    cmd.Parameters.AddWithValue("@ProductId", e.CommandArgument);
                    cmd.Parameters.AddWithValue("@Quantity", 1);
                    cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        Response.Write("<script>alert('Error - " + ex.Message + "')</script>");
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                else
                {
                    //Updating Existing Item in Cart
                    Utils utils = new Utils();
                    isCartItemUpdated = utils.updateCartQuantity(i + 1 ,Convert.ToInt32(e.CommandArgument), 
                        Convert.ToInt32(Session["userId"]));
                    lblMsg.Visible=true;
                    lblMsg.Text = "item Add Successfully in your Cart";
                    lblMsg.CssClass = "alert alert-success";
                    Response.AddHeader("REFRESH", "1;URL=Cart.aspx"); 
                }

            }
            else
            {
                Response.Redirect("Login.aspx");
            }
        }

        int isItemExistInCart(int productId)
        {
            con = new SqlConnection(Connection.GetConnectionString());
            cmd = new SqlCommand("Cart_Crud", con);
            cmd.Parameters.AddWithValue("@Action", "GETBYID");
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
            cmd.CommandType = CommandType.StoredProcedure;
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            int quantity = 0;
            if (dt.Rows.Count > 0)
            {
                quantity = Convert.ToInt32(dt.Rows[0]["Quantity"]);
            }
            return quantity;
        }
        //public string LowerCase(object obj)
        //{
        //  return obj.ToString().ToLower();
        //}
    }
}