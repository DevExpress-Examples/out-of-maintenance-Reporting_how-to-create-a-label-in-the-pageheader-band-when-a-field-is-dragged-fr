using DevExpress.Data.Browsing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Design;
using DevExpress.XtraReports.Expressions.Native;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UserDesigner;
using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace T457241 {

    public class CustomFieldListDragDropService : IFieldListDragDropService {
        private IDesignerHost host;
        private XRDesignPanel panel;

        public CustomFieldListDragDropService(IDesignerHost host, XRDesignPanel panel) {
            this.host = host;
            this.panel = panel;
        }
        public IDragHandler GetDragHandler() {
            return new CustomFieldDragHandler(host, panel);
        }
    }

    public class CustomFieldDragHandler : FieldDragHandler {
        XRDesignPanel panel;
        XRControl droppedControl;

        public CustomFieldDragHandler(IDesignerHost host, XRDesignPanel panel)
            : base(host) {
            this.host = host;
            this.panel = panel;
        }

        public override void HandleDragDrop(object sender, DragEventArgs e) {
            DataInfo[] droppedData = e.Data.GetData(typeof(DataInfo[])) as DataInfo[];
            XRControl parentControl = bandViewSvc.GetControlByScreenPoint(new Point(e.X, e.Y));

            ISelectionService selectSvc = host.GetService(typeof(ISelectionService)) as ISelectionService;

            if(((parentControl is XRPanel) || (parentControl is Band)) && ((droppedData.Length == 1))) {
                AddSingleField(e, droppedData, parentControl, selectSvc);
            } else if(((parentControl is XRPanel) || (parentControl is Band)) && ((droppedData.Length > 1)))
                AddMultipleFields(e, droppedData, parentControl, selectSvc);
            else
                base.HandleDragDrop(sender, e);
        }

        private void AddMultipleFields(DragEventArgs e, DataInfo[] droppedData, XRControl parentControl, ISelectionService selectSvc) {
            this.AdornerService.ResetSnapping();
            this.RulerService.HideShadows();
            XRTableCell headerCell;
            XRControl parent = bandViewSvc.GetControlByScreenPoint(new Point(e.X, e.Y));
            if(parent == null)
                return;
            SizeF size = new SizeF(100F * droppedData.Length, 25F);

            if(parentControl is DetailBand)
                size.Width = CalculateWidth(parentControl);

            XRTable detailTable = new XRTable() { Name = "DetailTable" };
            detailTable.BeginInit();
            XRTableRow detailRow = new XRTableRow();
            detailTable.Rows.Add(detailRow);

            this.droppedControl = detailTable;
            detailTable.SizeF = size;

            host.Container.Add(detailTable);
            host.Container.Add(detailRow);

            for(int i = 0; i < droppedData.Length; i++) {
                XRTableCell cell = new XRTableCell();
                string relatedDataMember = ExpressionBindingHelper.NormalizeDataMember(droppedData[i].Member);
                cell.ExpressionBindings.Add(new ExpressionBinding("Text",
                    relatedDataMember));
                detailRow.Cells.Add(cell);
                host.Container.Add(cell);
            }
            detailTable.EndInit();

            selectSvc.SetSelectedComponents(new XRControl[] { detailTable });

            PointF dropPoint = GetDragDropLocation(e, detailTable, parentControl);
            this.DropXRControl(parentControl, new PointF(0, dropPoint.Y));

            if((parentControl is DetailBand)) {
                PageHeaderBand band = null;
                if((parentControl as DetailBand).Report.Bands.OfType<PageHeaderBand>().FirstOrDefault() != null) {
                    band = (parentControl as DetailBand).Report.Bands[BandKind.PageHeader] as PageHeaderBand;
                } else {
                    band = new PageHeaderBand();
                    (parentControl as DetailBand).Report.Bands.Add(band);
                    host.Container.Add(band);
                }

                XRTable headerTable = new XRTable() { Name = "HeaderTable" };
                headerTable.BeginInit();

                XRTableRow headerRow = new XRTableRow();
                headerTable.Rows.Add(headerRow);

                headerTable.SizeF = size;

                host.Container.Add(headerTable);
                host.Container.Add(headerRow);

                for(int i = 0; i < droppedData.Length; i++)
                    headerCell = CreateTableCell(host, headerRow, droppedData[i].DisplayName);
                headerTable.Borders = BorderSide.All;
                headerTable.EndInit();

                band.Controls.Add(headerTable);
                headerTable.LocationF = droppedControl.LocationF;
            }
        }

        XRTableCell CreateTableCell(IDesignerHost host, XRTableRow row, string cellText) {
            XRTableCell headerCell = new XRTableCell();
            headerCell.Text = cellText;
            headerCell.BackColor = Color.Green;
            headerCell.ForeColor = Color.Yellow;
            headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            headerCell.Font = new Font("Calibry", 11, FontStyle.Bold);
            row.Cells.Add(headerCell);
            host.Container.Add(headerCell);
            return headerCell;
        }


        private PointF GetDragDropLocation(DragEventArgs e, XRControl control, XRControl parent) {
            PointF bandPoint = EvalBandPoint(e, parent.Band);
            bandPoint = bandViewSvc.SnapBandPoint(bandPoint, parent.Band, control, new XRControl[] { control });
            PointF screenPoint = bandViewSvc.ControlViewToScreen(bandPoint, parent.Band);
            return bandViewSvc.ScreenToControl(new RectangleF(screenPoint, SizeF.Empty), parent).Location;
        }

        private float CalculateWidth(XRControl control) {
            XtraReport report = control.RootReport;
            return GraphicsUnitConverter.Convert(report.PageWidth - report.Margins.Left - report.Margins.Right, report.Dpi, GraphicsDpi.HundredthsOfAnInch);
        }

        private void AddSingleField(DragEventArgs e, DataInfo[] droppedData, XRControl parentControl, ISelectionService selectSvc) {
            this.AdornerService.ResetSnapping();
            this.RulerService.HideShadows();

            SizeF size = new SizeF(100F, 25F);

            XRLabel detailLabel = new XRLabel();
            this.droppedControl = detailLabel;
            detailLabel.SizeF = size;

            host.Container.Add(detailLabel);
            PointF dropPoint = GetDragDropLocation(e, detailLabel, parentControl);
            detailLabel.ExpressionBindings.Add(new ExpressionBinding("Text", droppedData[0].Member));

            selectSvc.SetSelectedComponents(new XRControl[] { detailLabel });

            this.DropXRControl(parentControl, dropPoint);

            if((parentControl is DetailBand)) {
                PageHeaderBand band = null;
                if(panel.Report.Bands.OfType<PageHeaderBand>().FirstOrDefault() != null) {
                    band = panel.Report.Bands[BandKind.PageHeader] as PageHeaderBand;
                } else {
                    band = new PageHeaderBand();
                    panel.Report.Bands.Add(band);
                    host.Container.Add(band);
                }
                XRLabel headerLabel = CreateLabel(droppedControl.LocationF, size, droppedData[0].DisplayName);
                host.Container.Add(headerLabel);
                band.Controls.Add(headerLabel);
            }
        }

        XRLabel CreateLabel(PointF location, SizeF size, string labelText) {
            XRLabel headerLabel = new XRLabel();
            headerLabel.SizeF = size;
            headerLabel.LocationF = location;
            headerLabel.Text = labelText;
            headerLabel.BackColor = Color.Green;
            headerLabel.ForeColor = Color.Yellow;
            headerLabel.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            headerLabel.Font = new Font("Calibry", 11, FontStyle.Bold);
            return headerLabel;
        }

        void DropXRControl(XRControl parent, PointF dropPoint) {
            PointF screenPoint = dropPoint;
            parent.Controls.Add(droppedControl);
            droppedControl.LocationF = dropPoint;
        }
    }
}
