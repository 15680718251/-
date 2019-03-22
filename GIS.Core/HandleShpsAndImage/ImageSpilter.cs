using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OSGeo.GDAL;

namespace GIS.HandleShpsAndImage
{
    public class ImageSpilter
    {
        public byte[] Buffer;
        double[] transform;
        private string Imgpath;
        private string newPath;
        private string pro;
        private int sizeX;
        private int sizeY;
        private OSGeo.GDAL.ColorTable colorTable;

        public ImageSpilter(string paths,Size size,double [] _transform,OSGeo.GDAL.ColorTable ct,byte [] buffer,string proj)
        {
            //Imgpath = paths[0];
            //newPath = paths[1];

            this.newPath = paths;
            this.sizeX = size.Width;
            this.sizeY = size.Height;
            this.colorTable = ct;
            //int i = ct.GetCount();
            this.transform = _transform;
            this.Buffer = buffer;
            this.pro = proj;


            #region 全部放到参数里面
            //Gdal.AllRegister();
            //OSGeo.GDAL.Dataset dataset = Gdal.OpenShared(Imgpath, Access.GA_ReadOnly);
            //pro = dataset.GetProjectionRef();

            //Buffer =new byte[sizeX*sizeY];
            ////byte[] fff = new byte[sizeX * sizeY];
            ////OSGeo.GDAL.Band band = dataset.GetRasterBand(1);
            //dataset.ReadRaster(x, y, sizeX, sizeY, Buffer, sizeX, sizeY, 1, new int[] { 1 }, 0, 0, 0);
            ////band.ReadRaster(x, y, sizeX, sizeY, Buffer, sizeX, sizeY, 0, 0);

            ////band.Dispose();
            //dataset.Dispose();
            #endregion
        }

        public void SpliterWork()
        {

            Gdal.AllRegister();
            Driver dr = Gdal.GetDriverByName("HFA");
            dr.Create(newPath, sizeX, sizeY, 1, DataType.GDT_Byte, null);

            OSGeo.GDAL.Dataset ds=OSGeo.GDAL.Gdal.Open(newPath,Access.GA_Update);

            ds.SetProjection(pro);
            ds.SetGeoTransform(transform);

            Band band = ds.GetRasterBand(1);
            band.SetColorTable(colorTable);

            //for (int i = 0; i < sizeY; i++)
            //{
            //    for (int j = 0; j < sizeX; j++)
            //    {
            //        byte[] b = new byte[1] { Buffer[j + i * sizeY] };
            //        band.WriteRaster(j,i,1,1,b,1,1,0,0);
            //    }
            //}
            band.WriteRaster(0, 0, sizeX, sizeY, Buffer, sizeX, sizeY, 0, 0);


            band.Dispose();
            ds.Dispose();
            dr.Dispose();
        }
    }
}
