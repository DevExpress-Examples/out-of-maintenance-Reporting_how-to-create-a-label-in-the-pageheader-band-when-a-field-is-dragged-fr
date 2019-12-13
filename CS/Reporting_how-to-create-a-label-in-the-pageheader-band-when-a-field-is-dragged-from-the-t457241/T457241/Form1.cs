using DevExpress.XtraReports.Design;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UserDesigner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace T457241 {
    public partial class Form1 : DevExpress.XtraEditors.XtraForm {
        ReportDesignTool designer;
        IDesignerHost host;
        public Form1() {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e) {
            designer = new ReportDesignTool(new XtraReport1());
            designer.DesignRibbonForm.DesignMdiController.DesignPanelLoaded += DesignMdiController_DesignPanelLoaded;
            designer.ShowRibbonDesignerDialog();
        }

        private void DesignMdiController_DesignPanelLoaded(object sender, DesignerLoadedEventArgs e) {
            if(radioGroup1.SelectedIndex == 0)
                (sender as XRDesignPanel).ComponentAdded += DesignPanel_ComponentAdded;
            else {
                e.DesignerHost.RemoveService(typeof(IFieldListDragDropService));
                e.DesignerHost.AddService(typeof(IFieldListDragDropService), new CustomFieldListDragDropService(e.DesignerHost, (XRDesignPanel)sender));
            }
        }

        private void DesignPanel_ComponentAdded(object sender, System.ComponentModel.Design.ComponentEventArgs e) {
            PageHeaderBand headerBand;
            host = (IDesignerHost)designer.DesignRibbonForm.DesignMdiController.ActiveDesignPanel.Report.Site.GetService(typeof(IDesignerHost));
            if(!(e.Component is XRLabel) && !(e.Component is XRTable)) return;
            if(e.Component is XRLabel) {
                XRLabel label = e.Component as XRLabel;
                if(label.ExpressionBindings.Count == 0) return;
                if(label.Parent is XRTableRow) return;
                headerBand = CreatePageHeaderBand(designer.Report);
                XRLabel newLabel = CopyLabel(label, headerBand);
                headerBand.Controls.Add(newLabel);
                DesignToolHelper.AddToContainer(host, newLabel);
            }
            if(e.Component is XRTable) {
                headerBand = CreatePageHeaderBand(designer.Report);
                XRTableRow sourceRow = (e.Component as XRTable).Rows[0];
                XRTable headerTable = CreateTable(sourceRow, headerBand.HeightF);
                headerBand.Controls.Add(headerTable);
                DesignToolHelper.AddToContainer(host, headerTable);
            }
        }

        PageHeaderBand CreatePageHeaderBand(XtraReport report) {
            PageHeaderBand headerBand = report.Bands[BandKind.PageHeader] as PageHeaderBand;
            // Remove the comments if you need to recreate the PageHeader band
            //if(headerBand != null) {
            //    for(int i = headerBand.Controls.Count - 1; i >= 0; i--)
            //        DesignToolHelper.RemoveFromContainer(host, headerBand.Controls[i]);
            //    headerBand.Controls.Clear();
            //    DesignToolHelper.RemoveFromContainer(host, headerBand);
            if(headerBand == null) {
                headerBand = new PageHeaderBand();
                headerBand.HeightF = 0;
                headerBand.Visible = true;
                DesignToolHelper.AddToContainer(host, headerBand);
            }
            return headerBand;
        }

        XRTable CreateTable(XRTableRow sourceRow, float height) {
            XRTable table = new XRTable();
            XRTableRow headerTableRow = new XRTableRow();
            table.BeginInit();
            table.Rows.Add(headerTableRow);
            foreach(XRTableCell cell in sourceRow.Cells)
                CopyCell(cell, headerTableRow);
            table.Borders = DevExpress.XtraPrinting.BorderSide.All;
            table.LocationF = new PointF(0, height);
            table.AdjustSize();
            table.EndInit();
            return table;
        }

        XRLabel CopyLabel(XRLabel label, PageHeaderBand band) {
            string text = "XRLabel";
            XtraReport report = band.Report as XtraReport;
            XRLabel newLabel = new XRLabel();
            newLabel.LocationF = label.LocationF;
            newLabel.WidthF = label.WidthF;
            newLabel.HeightF = label.HeightF;
            newLabel.BackColor = Color.Green;
            newLabel.ForeColor = Color.Yellow;
            newLabel.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            newLabel.Font = new Font("Calibry", 11, FontStyle.Bold);
            newLabel.Borders = DevExpress.XtraPrinting.BorderSide.All;
            if(label.ExpressionBindings.Count > 0) {
                text = label.ExpressionBindings[0].Expression;
                int index = text.LastIndexOf(".");
                if(index > 0)
                    text = text.Substring(index + 1);
            }
            newLabel.Text = text;
            return newLabel;
        }

        void CopyCell(XRTableCell source, XRTableRow row) {
            XRTableCell cell = new XRTableCell();
            cell.WidthF = source.WidthF;
            cell.HeightF = source.HeightF;
            cell.BackColor = Color.Green;
            cell.ForeColor = Color.Yellow;
            cell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            cell.Font = new Font("Calibry", 11, FontStyle.Bold);
            string text = source.ExpressionBindings[0].Expression;
            int index = text.LastIndexOf(".");
            if(index > 0)
                text = text.Substring(index + 1);
            cell.Text = text;
            row.Cells.Add(cell);
            DesignToolHelper.AddToContainer(host, cell);
        }
    }
}
