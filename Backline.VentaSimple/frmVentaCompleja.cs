using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Backline.DAL;
using DevExpress.XtraBars;
using Backline.Entidades;
using DevExpress.Utils.Extensions;
using FacEleUtils;
using FacEleUtils.DoceleOLService;
using System.IO;

namespace Backline.VentaSimple
{
    public partial class frmVentaCompleja : Form
    {
        string _ambiente;
        Entidades.Contribuyente _contribuyenteEncontrado;        
        string _proveedor;
        FacEleClient client;
        List<Entidades.DetalleFactura> _detalleFactura = new List<DetalleFactura>();
        List<Entidades.Prestacion> _listaPrestaciones = new List<Prestacion>();
        public frmVentaCompleja()
        {
            _ambiente = System.Configuration.ConfigurationSettings.AppSettings["Ambiente"];
            _proveedor = System.Configuration.ConfigurationSettings.AppSettings["Proveedor"];
            string titulo = _ambiente == "cer" ? "TEST" : "PRODUCCIÓN";
            client = new FacEleClient();
            InitializeComponent();
            this.Text = "Backline - emisión de boletas (Ambiente:" + titulo + ") 20210930 3009";
            //Verifica Directorio Ordenes de Compra
            if (!System.IO.Directory.Exists("C:\\Documentos Backline"))
            {
                System.IO.Directory.CreateDirectory("C:\\Documentos Backline");
            }
            //Verifica Directorio Boletas
            if (!System.IO.Directory.Exists(@"C:\\Documentos Backline\BOLETAS"))
            {
                System.IO.Directory.CreateDirectory(@"C:\\Documentos Backline\BOLETAS");
            }
            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 1)
                pictureBox2.Image = picLareina.Image;
            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 2)
                pictureBox2.Image = picÑuñoa.Image;
            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 3)
                pictureBox2.Image = picProvi.Image;
            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 5)
                pictureBox2.Image = picCmvm.Image;
            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 6)
                pictureBox2.Image = picHualpen.Image;
            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 7)
            {
                pictureBox2.Image = picValpo.Image;
                pictureBox2.Width = picValpo.Width;
                pictureBox2.Height = picValpo.Height;
            }

            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 8)
            {
                pictureBox2.Image = picNavia.Image;
                pictureBox2.Width = picNavia.Width;
                pictureBox2.Height = picNavia.Height;
            }

            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 9)
                pictureBox2.Image = picPudahuel.Image;

            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 10)
                pictureBox2.Image = picBarnechea.Image;

            if (VariablesGlobales.UsuarioLogeado.Id==52)
            {
                btnGenerarRapido.Visible = true;
            }

            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 11)
                pictureBox2.Image = picOlmue.Image;

            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 12)
                pictureBox2.Image = picLautaro.Image;


            if (VariablesGlobales.UsuarioLogeado.Emp_Id == 15)
                pictureBox2.Image = picQuisco.Image;

            cmbTipoPago.Properties.DataSource = DAL.TipoPagoDAL.ObtenerTiposPago();
            _listaPrestaciones = DAL.PrestacionDAL.ObtenerPrestaciones(VariablesGlobales.UsuarioLogeado.Emp_Id);
            cmbPrestacion.Properties.DataSource = _listaPrestaciones;

            AgregaPrestaciónPorDefecto();
        }

        bool Valida()
        {
            dxErrorProvider1.ClearErrors();

            if (txtRut.Text == "")
            {
                dxErrorProvider1.SetError(txtRut, "Debe indicar el RUT");
            }
            if (txtRut.Text != "" && Utiles.ValidaRut(txtRut.Text) == false)
            {
                dxErrorProvider1.SetError(txtRut, "El RUT indicado no es válido");
            }
            if (txtNombre.Text == "")
            {
                dxErrorProvider1.SetError(txtNombre, "Debe indicar el nombre");
            }

          
            
            if (txtTotal.Value == 0)
            {
                dxErrorProvider1.SetError(txtTotal, "Debe indicar el detalle de las prestaciones");
            }
            if (cmbTipoPago.EditValue == null && chkNoPago.Checked == false)
            {
                dxErrorProvider1.SetError(cmbTipoPago, "Debe indicar el tipo de pago");
            }

            if (dxErrorProvider1.HasErrors)
                return false;

            return true;
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            if (Valida() == false)
                return;

            splashScreenManager3.ShowWaitForm();
            btnGenerar.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            Backline.Entidades.Contribuyente contribuyente = new Backline.Entidades.Contribuyente();
            contribuyente.Razon_Social = txtNombre.Text;
            contribuyente.Rut = txtRut.Text;
            contribuyente.Rut_Code = Utiles.ObtieneRut_INT(txtRut.Text);

            //Guardo Usuario si este no existe
            if (_contribuyenteEncontrado == null)
            {
                _contribuyenteEncontrado = ContribuyenteDAL.InsertarContribuyente(contribuyente);

            }

            Backline.DTE.APIResult apiResult = new Backline.DTE.APIResult();
            int folioSII = 0;
            string rutaPDF = string.Empty;
            bool validadaSII = false;
            List<Entidades.DetalleFactura> _detalleArticulo = new List<Entidades.DetalleFactura>();

            Backline.Entidades.Factura factura = new Backline.Entidades.Factura();
            factura.Rut = txtRut.Text.ToUpper();
            factura.RazonSocial = txtNombre.Text;
            factura.Total = int.Parse(txtCantidad.Value.ToString()) * int.Parse(txtValor.Value.ToString());
            factura.Neto = Math.Round(factura.Total / 1.19, 0);
            factura.Iva = Math.Round(factura.Total - factura.Neto);

            factura.Fecha = DateTime.Now;
            factura.ContId = _contribuyenteEncontrado.Id;
            factura.Glosa = txtMemo.Text ;
            factura.Usr_Id = VariablesGlobales.UsuarioLogeado.Id;
            factura.EmpId = VariablesGlobales.UsuarioLogeado.Emp_Id;
            factura.TiPaId = cmbTipoPago.EditValue != null ? (cmbTipoPago.EditValue as Entidades.TipoPago).Id : 0;
            factura.SinPago = chkNoPago.Checked;


            //Entidades.DetalleFactura detalle = new Entidades.DetalleFactura();
            //detalle.Cantidad = txtCantidad.Value;
            //detalle.DescripcionProducto = txtGlosa.Text;
            //detalle.Valor = txtValor.Value;

            //_detalleArticulo.Add(detalle);

            if (chkNoPago.Checked == false)
            {
                if (VariablesGlobales.UsuarioLogeado.Facturador == "superfactura")
                {
                    validadaSII = Utiles.GenerarBoletaElectronica(_detalleFactura, factura, Backline.DTE.Enums.TipoDocumento.BoletaElectronicaExenta, out folioSII, out rutaPDF, out apiResult);
                    factura.NumeroSII = folioSII;
                    //MessageBox.Show("Número" + folioSII.ToString());
                }
                if (VariablesGlobales.UsuarioLogeado.Facturador == "facele")
                {
                    generaDTEFormato formato = generaDTEFormato.XML;
                    string xml = VariablesGlobales.UsuarioLogeado.EsAfecta == true ? obtieneXMLBoleta(factura) : obtieneXMLBoletaExcenta(factura);

                    string descripcionOperacion;
                    long folio;
                    string rut = VariablesGlobales.UsuarioLogeado.RutEmpresa;
                    int tipoDTE = VariablesGlobales.UsuarioLogeado.EsAfecta == true ? 39 : 41;
                    var salida = client.generaDTE(ref rut, ref tipoDTE, formato, "", xml, null, out descripcionOperacion, out folio);

                    if (descripcionOperacion != "Proceso OK")
                    {
                        btnGenerar.Enabled = true;
                        this.Cursor = Cursors.Default;
                        splashScreenManager3.CloseWaitForm();
                        MessageBox.Show("ERROR al generar boleta:" + descripcionOperacion, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    factura.NumeroSII = folio;
                    rutaPDF = ObtenerBoleta(folio, tipoDTE);
                }

                if (rutaPDF == null || rutaPDF == "")
                {
                    MessageBox.Show("La ruta del PDF es nula", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

           
            
            factura = Backline.DAL.BoletaDAL.InsertarFactura(factura);
            btnGenerar.Enabled = true;
            splashScreenManager3.CloseWaitForm();
            this.Cursor = Cursors.Default;
            if (chkNoPago.Checked == false)
            {
                Visualizador frmVisualiza = new Visualizador();
                frmVisualiza.CargarPDF(rutaPDF);
                frmVisualiza.ShowDialog();
            }
           

            AccionLimpiar();
            MessageBox.Show("El movimiento se ha guardado con éxito", "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        string ObtenerBoleta(long numero, int TipoDTE)
        {
            var filename = Guid.NewGuid().ToString() + ".pdf";
            var fileLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            string descripcionOperacion;
            string rut = VariablesGlobales.UsuarioLogeado.RutEmpresa;
            int tipoDTE;
            int.TryParse(TipoDTE.ToString(), out tipoDTE);
            long folioDTE;
            long.TryParse(numero.ToString(), out folioDTE);
            byte[] PDF;
            string XML;
            string URL;
            client.obtienePDF(rut, tipoDTE, folioDTE, out descripcionOperacion, out XML, out PDF, out URL);
            client.grabaPDF(PDF, fileLocation);
            client.obtieneURLPDF(rut, tipoDTE, folioDTE, out descripcionOperacion, out XML, out PDF, out URL);
            //lblLinkPDF.Enabled = true;
            //lblLinkPDF.Text = URL;
            return fileLocation;
        }

        private string obtieneXMLBoleta(Entidades.Factura factura)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<DTE version=\"1.0\">");
            sb.Append("<Documento>");
            sb.Append("<Encabezado>");
            sb.Append("<IdDoc>");
            sb.Append("<TipoDTE>39</TipoDTE>");
            sb.Append("<Folio>0</Folio>");
            sb.Append("<FchEmis>" + FacEleUtils.Utiles.ObtenerFecha(DateTime.Now) + "</FchEmis>");
            sb.Append("<IndServicio>1</IndServicio>");
            sb.Append("<IndMntNeto>2</IndMntNeto>");
            sb.Append("</IdDoc>");
            sb.Append("<Emisor>");
            sb.Append("<RUTEmisor>" + VariablesGlobales.UsuarioLogeado.RutEmpresa + "</RUTEmisor>");
            sb.Append("<RznSocEmisor>" + VariablesGlobales.UsuarioLogeado.NombreEmpresa + "</RznSocEmisor>");
            sb.Append("<GiroEmisor>Giro</GiroEmisor>");
            sb.Append("<DirOrigen>Direccion</DirOrigen>");
            sb.Append("<CmnaOrigen>La Reina</CmnaOrigen>");
            sb.Append("<CiudadOrigen>Santiago</CiudadOrigen>");
            sb.Append("</Emisor>");
            sb.Append("<Receptor>");
            sb.Append("<RUTRecep>" + factura.Rut + "</RUTRecep>");
            sb.Append("<RznSocRecep>" + factura.RazonSocial + "</RznSocRecep>");
            sb.Append("<Contacto>contacto</Contacto>");
            sb.Append("<DirRecep>...</DirRecep>");
            sb.Append("<CmnaRecep>La Reina</CmnaRecep>");
            sb.Append("<CiudadRecep>Santiago</CiudadRecep>");
            sb.Append("</Receptor>");
            sb.Append("<Totales>");
            sb.Append("<MntNeto>" + factura.Neto.ToString() + "</MntNeto>");
            sb.Append("<MntExe>0</MntExe>");
            sb.Append("<IVA>" + factura.Iva.ToString() + "</IVA>");
            sb.Append("<MntTotal>" + factura.Total.ToString() + "</MntTotal>");
            sb.Append("<VlrPagar>" + factura.Total.ToString() + "</VlrPagar>");
            sb.Append("</Totales>");
            sb.Append("</Encabezado>");

            int contador = 1;
            foreach(var a in _detalleFactura )
            {
                sb.Append("<Detalle>");
                sb.Append("<NroLinDet>" + contador.ToString() + "</NroLinDet>");
                sb.Append("<NmbItem>" + a.DescripcionProducto + "</NmbItem>");
                sb.Append("<QtyItem>" + a.Cantidad.ToString() + "</QtyItem>");
                sb.Append("<UnmdItem></UnmdItem>");
                sb.Append("<PrcItem>" + a.Valor.ToString() + "</PrcItem>");
                sb.Append("<MontoItem>" + a.Subtotal.ToString() + "</MontoItem>");
                sb.Append("</Detalle>");
                contador++;
            }

            


            sb.Append("</Documento>");
            sb.Append("</DTE>");

            return sb.ToString();
        }

        private string obtieneXMLBoletaExcenta(Entidades.Factura factura)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<DTE version=\"1.0\">");
            sb.Append("<Documento>");
            sb.Append("<Encabezado>");
            sb.Append("<IdDoc>");
            sb.Append("<TipoDTE>41</TipoDTE>");
            sb.Append("<Folio>0</Folio>");
            sb.Append("<FchEmis>" + FacEleUtils.Utiles.ObtenerFecha(DateTime.Now) + "</FchEmis>");
            sb.Append("<IndServicio>1</IndServicio>");

            sb.Append("</IdDoc>");
            sb.Append("<Emisor>");
            sb.Append("<RUTEmisor>"  + VariablesGlobales.UsuarioLogeado.RutEmpresa  + "</RUTEmisor>");
            sb.Append("<RznSocEmisor>" + VariablesGlobales.UsuarioLogeado.NombreEmpresa + "</RznSocEmisor>");
            sb.Append("<GiroEmisor>Giro</GiroEmisor>");
            sb.Append("<DirOrigen>Direccion</DirOrigen>");
            sb.Append("<CmnaOrigen>La Reina</CmnaOrigen>");
            sb.Append("<CiudadOrigen>Santiago</CiudadOrigen>");
            sb.Append("</Emisor>");
            sb.Append("<Receptor>");
            sb.Append("<RUTRecep>" + factura.Rut + "</RUTRecep>");
            sb.Append("<RznSocRecep>" + factura.RazonSocial + "</RznSocRecep>");
            sb.Append("<Contacto>contacto</Contacto>");
            sb.Append("<DirRecep>...</DirRecep>");
            sb.Append("<CmnaRecep>La Reina</CmnaRecep>");
            sb.Append("<CiudadRecep>Santiago</CiudadRecep>");
            sb.Append("</Receptor>");
            sb.Append("<Totales>");

            sb.Append("<MntExe>" + factura.Total.ToString() + "</MntExe>");
            //sb.Append("<IVA>0</IVA>");
            sb.Append("<MntTotal>" + factura.Total.ToString() + "</MntTotal>");
            sb.Append("<VlrPagar>0</VlrPagar>");
            sb.Append("</Totales>");
            sb.Append("</Encabezado>");


            int contador = 1;
            foreach(var a in _detalleFactura)
            {
                sb.Append("<Detalle>");
                sb.Append("<NroLinDet>" + contador.ToString() + "</NroLinDet>");
                sb.Append("<IndExe>1</IndExe>");
                sb.Append("<NmbItem>" + a.DescripcionProducto + "</NmbItem>"); ;
                sb.Append("<QtyItem>" + a.Cantidad.ToString() + "</QtyItem>");
                sb.Append("<UnmdItem/>");
                sb.Append("<PrcItem>" + a.Valor.ToString() + "</PrcItem>");
                sb.Append("<MontoItem>" + a.Subtotal + "</MontoItem>");
                sb.Append("</Detalle>");
                contador++;
            }

           

            sb.Append("</Documento>");
            sb.Append("</DTE>");


            return sb.ToString();
        }

        //static string ObtieneCiudad()
        //{
        //    //La Reina
        //    switch (PortalGestion.Entidades.VariablesGlobales.EmpresaSeleccionada.Id)
        //    {
        //        case 31:
        //            return "Santiago";
        //    }
        //    //Constitución
        //    switch (PortalGestion.Entidades.VariablesGlobales.EmpresaSeleccionada.Id)
        //    {
        //        case 48:
        //            return "Constitución";
        //    }

        //    return "";
        //}
        //static string ObtieneComuna()
        //{
        //    //La Reina
        //    switch (PortalGestion.Entidades.VariablesGlobales.EmpresaSeleccionada.Id)
        //    {
        //        case 31:
        //            return "La Reina";
        //    }
        //    //Constitución
        //    switch (PortalGestion.Entidades.VariablesGlobales.EmpresaSeleccionada.Id)
        //    {
        //        case 48:
        //            return "Constitución";
        //    }

        //    return "";
        //}

        public static int Valor_REDONDEO_INT_ARRIBA(int valor)
        {
            int ultimo = int.Parse(valor.ToString().Substring(valor.ToString().Length - 1, 1));

            if (ultimo >= 6)
            {
                return valor = valor + (10 - ultimo);
            }
            if (ultimo < 6)
            {
                return valor = valor - (ultimo);
            }

            return 0;
        }

        private void txtRut_Leave(object sender, EventArgs e)
        {
            txtRut.Text = Utiles.FormateaRut(txtRut.Text);
            if (!Utiles.ValidaRut(txtRut.Text))
            {
                txtRut.Focus();
                return;
            }

            if (txtRut.Text.Trim() == string.Empty)
                return;


            Entidades.Filtro filtro = new Entidades.Filtro();
            filtro.RutCode = Utiles.ObtieneRut_INT(txtRut.Text);
            var resultado = DAL.ContribuyenteDAL.ObtenerContribuyente(filtro);
            if (resultado != null && resultado.Count == 1)
            {
                txtNombre.Text = resultado[0].Razon_Social;
                _contribuyenteEncontrado = resultado[0];
            }
            else
            {
                txtNombre.Text = "";
                _contribuyenteEncontrado = null;
            }

        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            AccionLimpiar();
        }

        void AccionLimpiar()
        {
            _detalleFactura.Clear();
            txtRut.Text = "";
            txtNombre.Text = "";
            txtMemo.Text = "";
            txtNumeroAtencion.Text = "";
            chkNoPago.Checked = false;

            txtCantidad.Value = 1;
            //txtGlosa.Text = "";
            txtValor.Value = 0;
            cmbTipoPago.EditValue = null;

            txtValor.ReadOnly = true;

            AgregaPrestaciónPorDefecto();
        }

        private void txtRut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtNombre.Focus();
            }
        }

        private void btnGenerarRapido_Click(object sender, EventArgs e)
        {
           

            splashScreenManager3.ShowWaitForm();
            btnGenerar.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            Backline.Entidades.Contribuyente contribuyente = new Backline.Entidades.Contribuyente();
            contribuyente.Razon_Social = txtNombre.Text;
            contribuyente.Rut = txtRut.Text;
            contribuyente.Rut_Code = Utiles.ObtieneRut_INT(txtRut.Text);

            //Guardo Usuario si este no existe
            if (_contribuyenteEncontrado == null)
            {
                _contribuyenteEncontrado = ContribuyenteDAL.InsertarContribuyente(contribuyente);

            }

            Backline.DTE.APIResult apiResult = new Backline.DTE.APIResult();
            int folioSII = 0;
            string rutaPDF = string.Empty;
            bool validadaSII = false;
            List<Entidades.DetalleFactura> _detalleArticulo = new List<Entidades.DetalleFactura>();

            Backline.Entidades.Factura factura = new Backline.Entidades.Factura();
            factura.Rut = "";
            factura.RazonSocial = "";
            factura.Total = 700;
            factura.Neto = 588;
            factura.Iva = 112;

            factura.Fecha = DateTime.Now;
            factura.ContId = _contribuyenteEncontrado.Id;
            factura.Glosa = "Atención Ñuñoa";
            factura.Usr_Id = VariablesGlobales.UsuarioLogeado.Id;
            factura.EmpId = VariablesGlobales.UsuarioLogeado.Emp_Id;
            factura.EstId = VariablesGlobales.UsuarioLogeado.Est_Id;

            Entidades.DetalleFactura detalle = new Entidades.DetalleFactura();
            detalle.Cantidad = 1;
            detalle.DescripcionProducto = "Peaje cementerio";
            detalle.Valor = 700;


            _detalleArticulo.Add(detalle);

            if (VariablesGlobales.UsuarioLogeado.Facturador == "superfactura")
            {
                validadaSII = Utiles.GenerarBoletaElectronicaRapida(_detalleArticulo, factura, Backline.DTE.Enums.TipoDocumento.BoletaElectronicaExenta, out folioSII, out rutaPDF, out apiResult);
                factura.NumeroSII = folioSII;
                //MessageBox.Show("Número" + folioSII.ToString());
            }


            if (rutaPDF == null || rutaPDF == "")
            {
                MessageBox.Show("La ruta del PDF es nula", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            factura = Backline.DAL.BoletaDAL.InsertarFactura(factura);
            btnGenerar.Enabled = true;
            splashScreenManager3.CloseWaitForm();
            this.Cursor = Cursors.Default;
            Visualizador frmVisualiza = new Visualizador();
            frmVisualiza.CargarPDF(rutaPDF);
            frmVisualiza.ShowDialog();

            AccionLimpiar();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            Entidades.Prestacion prestacion = cmbPrestacion.EditValue as Entidades.Prestacion;

            Entidades.DetalleFactura detalle = new DetalleFactura();
            detalle.DescripcionProducto = prestacion.Descripcion;
            detalle.Cantidad = decimal.Parse(txtCantidad.Value.ToString());
            detalle.Valor = decimal.Parse(txtValor.Value.ToString());
            detalle.Subtotal = detalle.Cantidad * detalle.Valor;
            
            _detalleFactura.Add(detalle);

            Actualiza();

            cmbPrestacion.EditValue = "";
            txtCantidad.Text = "1";
            txtValor.Text = "0;";
            txtSubTotal.Text = "0";

                

        }

        void Actualiza()
        {
            decimal subTotal = 0;
            foreach(var a in _detalleFactura)
            {
                subTotal = subTotal + a.Subtotal;
            }

            decimal neto = subTotal / decimal.Parse("1,19");
            decimal iva = subTotal - neto;

            //txtNeto.Value = neto;
            //txtIva.Value = iva;
            txtTotal.Value = subTotal;

            grdDatos.DataSource = _detalleFactura;
            grdDatos.MainView.LayoutChanged();
        }

        private void cmbPrestacion_EditValueChanged(object sender, EventArgs e)
        {
            Entidades.Prestacion pres = cmbPrestacion.EditValue as Entidades.Prestacion;
            if (pres != null)
            {
                txtValor.Value = decimal.Parse(pres.Valor.ToString());
                txtSubTotal.Value = txtCantidad.Value * txtValor.Value;

                if (pres.ValorLibre)
                {
                    txtValor.ReadOnly = false;
                }
                else
                {
                    txtValor.ReadOnly = true;
                }
            }
        }

        private void txtCantidad_EditValueChanged(object sender, EventArgs e)
        {
            Calcular();
        }

        void Calcular()
        {
            txtSubTotal.Value = txtCantidad.Value * txtValor.Value;
        }

        private void repDel_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            Entidades.DetalleFactura detalle = gvDatos.GetRow(gvDatos.FocusedRowHandle) as Entidades.DetalleFactura;
            if (detalle != null)
            {
                if (DialogResult.OK == MessageBox.Show("¿Desea remover del listado la prestación " + detalle.DescripcionProducto + "?","Remover", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
                {
                    _detalleFactura.Remove(detalle);
                    Actualiza();
                }

            }

            
        }

        void AgregaPrestaciónPorDefecto()
        {
            Entidades.Prestacion prestacion = _listaPrestaciones.Where(d => d.Id == 1).FirstOrDefault();


            Entidades.DetalleFactura detalle = new DetalleFactura();
            detalle.Id = 1;
            detalle.DescripcionProducto = prestacion.Descripcion;
            detalle.Valor = prestacion.Valor;
            detalle.Cantidad = 1;
            detalle.Subtotal = prestacion.Valor;
           
            _detalleFactura.Add(detalle);

            Actualiza();
        }

        private void txtMemo_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void chkNoPago_EditValueChanged(object sender, EventArgs e)
        {
            if (chkNoPago.Checked == true)
            {
                cmbTipoPago.Text = "SIN PAGO";
                cmbTipoPago.EditValue = null;
                cmbTipoPago.Enabled = false;
            }
            else
            {
                cmbTipoPago.Enabled = true;
                cmbTipoPago.EditValue = null;
            }
            
        }

        private void txtValor_EditValueChanged(object sender, EventArgs e)
        {
            Calcular();
        }
    }
}
