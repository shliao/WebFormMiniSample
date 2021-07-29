﻿using AccountingNote.DBSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AccountingNote.SystemAdmin
{
    public partial class AccountingDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //check is logined
            if (this.Session["UserLoginInfo"] == null)
            {
                Response.Redirect("/Login.aspx");
                return;
            }

            string account = this.Session["UserLoginInfo"] as string;
            var drUserInfo = UserInfoManager.GetUserInfoByAccount(account);

            if (drUserInfo == null)
            {
                Response.Redirect("/Login.aspx");
                return;
            }

            if (!this.IsPostBack)
            {
                //check is creat mode or edit mode.
                if (this.Request.QueryString["ID"] == null)
                {
                    this.btnDelete.Visible = false;
                }
                else
                {
                    this.btnDelete.Visible = true;

                    string idText = this.Request.QueryString["ID"];
                    int id;
                    if (int.TryParse(idText, out id))
                    {
                        var drAccounting = AccountingManager.GetAccounting(id, drUserInfo["ID"].ToString());

                        if (drAccounting == null)
                        {
                            this.ltMsg.Text = "Data dosen't exist";
                            this.btnSave.Visible = false;
                            this.btnDelete.Visible = false;
                        }
                        else
                        {
                            if (drAccounting["UserID"].ToString() == drUserInfo["ID"].ToString())
                            {
                                this.ddlActType.SelectedValue = drAccounting["ActType"].ToString();
                                this.txtAmount.Text = drAccounting["Amount"].ToString();
                                this.txtCaption.Text = drAccounting["Caption"].ToString();
                                this.txtDesc.Text = drAccounting["Body"].ToString();
                            }
                        }
                    }
                    else
                    {
                        this.ltMsg.Text = "Data dosen't exist";
                        this.btnSave.Visible = false;
                        this.btnDelete.Visible = false;
                    }
                }
            }
        }

        private bool CheckInput(out List<string> errorMMsgList)
        {
            List<string> msgList = new List<string>();

            //Type
            if (this.ddlActType.SelectedValue != "0" &&
                this.ddlActType.SelectedValue != "1")
            {
                msgList.Add("Type must be 0 or 1.");
            }

            //Amount
            if (string.IsNullOrWhiteSpace(this.txtAmount.Text))
            {
                msgList.Add("Amount is required.");
            }
            else
            {
                int tempInt;
                if (!int.TryParse(this.txtAmount.Text, out tempInt))
                {
                    msgList.Add("Amount must be a number.");
                }

                if (tempInt < 0 || tempInt > 1000000)
                {
                    msgList.Add("Amount must between  0 and 1,000,000.");
                }
            }

            errorMMsgList = msgList;

            if (msgList.Count == 0)
                return true;
            else
                return false;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            List<string> msgList = new List<string>();
            if (!this.CheckInput(out msgList))
            {
                this.ltMsg.Text = string.Join("<br/>", msgList);
                return;
            }

            //check is logined
            if (this.Session["UserLoginInfo"] == null)
            {
                Response.Redirect("/Login.aspx");
                return;
            }

            string account = this.Session["UserLoginInfo"] as string;
            var dr = UserInfoManager.GetUserInfoByAccount(account);

            if (dr == null)
            {
                Response.Redirect("/Login.aspx");
                return;
            }

            string userID = dr["ID"].ToString();
            string actTypeText = this.ddlActType.SelectedValue;
            string amountText = this.txtAmount.Text;
            string caption = this.txtCaption.Text;
            string body = this.txtDesc.Text;

            int amount = Convert.ToInt32(amountText);
            int actType = Convert.ToInt32(actTypeText);


            string idText = this.Request.QueryString["ID"];
            if (string.IsNullOrWhiteSpace(idText))
            {
                // Excute 'Insetr into db'
                AccountingManager.CreatAccounting(userID, caption, amount, actType, body);
            }
            else
            {
                int id;
                if (int.TryParse(idText, out id))
                {
                    AccountingManager.UpdateAccounting(id, userID, caption, amount, actType, body);
                }
            }

            Response.Redirect("/SystemAdmin/AccountingList.aspx");
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            string idText = this.Request.QueryString["ID"];
            if (string.IsNullOrWhiteSpace(idText))
                return;

            int id;
            if (int.TryParse(idText, out id))
            {
                AccountingManager.DeleteAccounting(id);
            }

            Response.Redirect("/SystemAdmin/AccountingList.aspx");
        }
    }
}