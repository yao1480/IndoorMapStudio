using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DXFReader.DataModel;
using System.IO;
using MathExtension.Vector;

namespace DXFReader.Reader
{
    public class DxfReader
    {
        private StreamReader streamReader = null;
        private GroupCode groupCode;
        public List<string> LayerNames = null;
        public List<LineClass> Lines = null;
        public List<LwPolyLineClass> LwPolylines = null;
        public List<CircleClass> Circles = null;
        public List<ArcClass> Arcs = null;
        public List<EllipseClass> Ellipses = null;
        public List<BlockClass> Blocks = null;
        public List<InsertClass> Inserts = null;
        public List<TextClass> Texts = null;

        #region 构造器
        public DxfReader(string dxfFilePath,bool mergeLines)
        {
            LayerNames = new List<string>();
            Lines = new List<LineClass>();
            Circles = new List<CircleClass>();
            Ellipses = new List<EllipseClass>();
            Arcs = new List<ArcClass>();
            LwPolylines = new List<LwPolyLineClass>();
            Blocks = new List<BlockClass>();
            Inserts = new List<InsertClass>();
            Texts = new List<TextClass>();

            ReadDxf(dxfFilePath);


            if (mergeLines)
                MergeLines();
        }
        #endregion

        #region 内部方法
        private void ReadDxf(string dxfFilePath)
        {
            streamReader = new StreamReader(dxfFilePath);

            bool canExit = false;

            while (streamReader.Peek() != -1)
            {
                groupCode = ReadGroupCode();

                if (groupCode.Value == "SECTION")
                {
                    groupCode = ReadGroupCode();
                    switch (groupCode.Value)
                    {
                        case "TABLES":
                            readTables();
                            break;

                        case "BLOCKS":
                            readBlocks();
                            break;

                        case "ENTITIES":
                            readEntities();
                            canExit = true;
                            break;
                    }
                }

                if (canExit)
                    break;
            }
            streamReader.Close();
        }

        #region TABLE SECTION
        private void readTables()
        {
            groupCode = ReadGroupCode();
            while (groupCode.Value != "ENDSEC")
            {
                if (groupCode.Code == "2")
                {
                    switch (groupCode.Value)
                    {
                        case "LAYER":
                            getLayerNames();
                            break;
                    }
                }
                groupCode = ReadGroupCode();
            }
        }

        private void getLayerNames()
        {
            groupCode = ReadGroupCode();
            while (groupCode.Value != "ENDTAB")
            {
                if (groupCode.Value == "AcDbLayerTableRecord")
                {
                    groupCode = ReadGroupCode();
                    LayerNames.Add(groupCode.Value);
                }
                groupCode = ReadGroupCode();
            }
        }
        #endregion

        #region BLOCKS SECTION
        private void readBlocks()
        {
            groupCode = ReadGroupCode();
            while (groupCode.Value != "ENDSEC")
            {
                if (groupCode.Value == "BLOCK")
                {
                    BlockClass pBlock = new BlockClass();
                    groupCode = ReadGroupCode();

                    bool hasReadBlockEntities;
                    while (groupCode.Value != "ENDBLK")
                    {
                        hasReadBlockEntities = false;
                        switch (groupCode.Code)
                        {
                            case "2":
                                pBlock.Name = groupCode.Value;
                                break;
                            case "8":
                                pBlock.LayerName = groupCode.Value;
                                break;
                            case "10":
                                pBlock.BasePoint.X = double.Parse(groupCode.Value);
                                break;
                            case "20":
                                pBlock.BasePoint.Y = double.Parse(groupCode.Value);
                                break;
                            case "30":
                                pBlock.BasePoint.Z = double.Parse(groupCode.Value);
                                break;
                            case "0":
                                readBlockEntities(ref pBlock);
                                hasReadBlockEntities = true;
                                break;

                        }
                        if (!hasReadBlockEntities)
                            groupCode = ReadGroupCode();
                    }
                    Blocks.Add(pBlock);
                }
                groupCode = ReadGroupCode();
            }
        }

        private void readBlockEntities(ref BlockClass pBlock)
        {
            bool hasReadGraphUnit;

            while (groupCode.Value != "ENDBLK")
            {
                hasReadGraphUnit = false;

                if (groupCode.Code == "0")
                {
                    switch (groupCode.Value)
                    {
                        case "LINE":
                            LineClass pLine = readLine();
                            pBlock.Entity.Lines.Add(pLine);
                            hasReadGraphUnit = true;
                            break;
                        case "LWPOLYLINE":
                            LwPolyLineClass pLwPolyline = readLwPolyline();
                            pBlock.Entity.LwPolylines.Add(pLwPolyline);
                            hasReadGraphUnit = true;
                            break;
                        //case "MLINE":
                        //    hasReadGraphUnit = true;
                        //    break;
                        case "CIRCLE":
                            CircleClass pCircle = readCircle();
                            pBlock.Entity.Circles.Add(pCircle);
                            hasReadGraphUnit = true;
                            break;

                        case "ELLIPSE":
                            EllipseClass pEllipse = readEllipse();
                            pBlock.Entity.Ellipses.Add(pEllipse);
                            hasReadGraphUnit = true;
                            break;

                        case "ARC":
                            ArcClass pArc = readArc();
                            pBlock.Entity.Arcs.Add(pArc);
                            hasReadGraphUnit = true;
                            break;
                    }
                }

                //由于图元是以组码0结束的，因此若读到是不应该再读取的，而是继续使用上一个图元末尾的组码值
                if (!hasReadGraphUnit)
                    groupCode = ReadGroupCode();
            }
        }


        #endregion

        #region Entities SECTION
        private void readEntities()
        {
            groupCode = ReadGroupCode();
            bool hasReadGraphUnit;//标志是否读取图元

            while (groupCode.Value != "ENDSEC")
            {
                hasReadGraphUnit = false;

                if (groupCode.Code == "0")
                {
                    switch (groupCode.Value)
                    {
                        case "LINE":
                            LineClass pLine = readLine();
                            Lines.Add(pLine);
                            hasReadGraphUnit = true;
                            break;
                        case "LWPOLYLINE":
                            LwPolyLineClass pLwPolyline = readLwPolyline();
                            LwPolylines.Add(pLwPolyline);
                            hasReadGraphUnit = true;
                            break;
                        //case "MLINE":
                        //    hasReadGraphUnit = true;
                        //    break;
                        case "CIRCLE":
                            CircleClass pCircle = readCircle();
                            Circles.Add(pCircle);
                            hasReadGraphUnit = true;
                            break;

                        case "ELLIPSE":
                            EllipseClass pEllipse = readEllipse();
                            Ellipses.Add(pEllipse);
                            hasReadGraphUnit = true;
                            break;

                        case "ARC":
                            ArcClass pArc = readArc();
                            Arcs.Add(pArc);
                            hasReadGraphUnit = true;
                            break;

                        case "INSERT":
                            InsertClass pInsert = readInsert();
                            Inserts.Add(pInsert);
                            hasReadGraphUnit = true;
                            break;

                        case "TEXT":
                        case "MTEXT":
                            TextClass pText = readText();
                            Texts.Add(pText);
                            hasReadGraphUnit = true;
                            break;

                    }
                }

                //由于图元是以组码0结束的，因此若读到是不应该再读取的，而是继续使用上一个图元末尾的组码值
                if (!hasReadGraphUnit)
                    groupCode = ReadGroupCode();
            }
        }

        private LineClass readLine()
        {
            LineClass pLine = new LineClass();

            groupCode = ReadGroupCode();
            while (groupCode.Code != "0") //以下一个图元开始符0为结束标志
            {
                switch (groupCode.Code)
                {
                    case "8":
                        pLine.LayerName = groupCode.Value;
                        break;
                    case "10":
                        pLine.StartPoint.X = double.Parse(groupCode.Value);
                        break;
                    case "20":
                        pLine.StartPoint.Y = double.Parse(groupCode.Value);
                        break;
                    case "30":
                        pLine.StartPoint.Z = double.Parse(groupCode.Value);
                        break;
                    case "11":
                        pLine.EndPoint.X = double.Parse(groupCode.Value);
                        break;
                    case "21":
                        pLine.EndPoint.Y = double.Parse(groupCode.Value);
                        break;
                    case "31":
                        pLine.EndPoint.Z = double.Parse(groupCode.Value);
                        break;
                }
                groupCode = ReadGroupCode();

            }
            return pLine;
        }

        private LwPolyLineClass readLwPolyline()
        {
            LwPolyLineClass pLwPolyline = new LwPolyLineClass();

            groupCode = ReadGroupCode();

            int vertexIndex = 0;//顶点计数

            while (groupCode.Code != "0") //以下一个图元开始符0为结束标志
            {
                switch (groupCode.Code)
                {
                    case "8":
                        pLwPolyline.LayerName = groupCode.Value;
                        break;

                    case "70":
                        pLwPolyline.CloseFlag = int.Parse(groupCode.Value);
                        break;

                    //10、20采集顶点集合
                    case "10":
                        pLwPolyline.PointCollection.Add(new PointStruct());
                        pLwPolyline.PointCollection[vertexIndex].X = double.Parse(groupCode.Value);

                        break;
                    case "20":
                        pLwPolyline.PointCollection[vertexIndex].Y = double.Parse(groupCode.Value);
                        vertexIndex++;
                        break;
                }
                groupCode = ReadGroupCode();
            }

            /*因为CAD只会将使用命令如Rec\Pol得到的外观闭合图形的组码70设为1（闭合），
             * 而对于使用PL命令时绘制图形并手工使其闭合的图形，其组码70=0
             * 并且会追加与起点值完全相同的点作为终点
             * 因此，对于手工绘制的闭合点，终点需要移除；考虑到绘图时存在终点与起点很相近，但不是严格的重合，
             * 对于这种情况，需设置阈值条件以判断其是否可以视为与起点相同的点，若是则一并删除；
             * 对外观闭合的点应该将闭合属性CloseFlag修改为1；
            */
            int pointCount = pLwPolyline.PointCollection.Count;
            if (pointCount > 2 && pLwPolyline.CloseFlag != 1)
            {
                //计算起终点距离
                double distance = MathExtension.Geometry.GeometryHelper.Calc_Distance(
                    pLwPolyline.PointCollection[pointCount - 1],pLwPolyline.PointCollection[0]);

                if (distance < 0.001)
                {
                    //距离小于千分之一毫米视为同一个点，即终点和起点重合

                    //移除重复的终点
                    pLwPolyline.PointCollection.RemoveAt(pointCount - 1);

                    //将多段线设为闭合
                    if (pLwPolyline.PointCollection.Count > 2)
                        pLwPolyline.CloseFlag = 1;
                }
            }
            return pLwPolyline;
        }

        private CircleClass readCircle()
        {
            CircleClass pCircle = new CircleClass();

            groupCode = ReadGroupCode();
            while (groupCode.Code != "0") //以下一个图元开始符0为结束标志
            {
                switch (groupCode.Code)
                {
                    case "8":
                        pCircle.LayerName = groupCode.Value;
                        break;
                    case "10":
                        pCircle.Centre.X = double.Parse(groupCode.Value);
                        break;
                    case "20":
                        pCircle.Centre.Y = double.Parse(groupCode.Value);
                        break;
                    case "30":
                        pCircle.Centre.Z = double.Parse(groupCode.Value);
                        break;
                    case "40":
                        pCircle.Radius = double.Parse(groupCode.Value);
                        break;
                }
                groupCode = ReadGroupCode();

            }
            return pCircle;
        }

        private ArcClass readArc()
        {
            ArcClass pArc = new ArcClass();

            groupCode = ReadGroupCode();
            while (groupCode.Code != "0") //以下一个图元开始符0为结束标志
            {
                switch (groupCode.Code)
                {
                    case "8":
                        pArc.LayerName = groupCode.Value;
                        break;
                    case "10":
                        pArc.Centre.X = double.Parse(groupCode.Value);
                        break;
                    case "20":
                        pArc.Centre.Y = double.Parse(groupCode.Value);
                        break;
                    case "30":
                        pArc.Centre.Z = double.Parse(groupCode.Value);
                        break;
                    case "40":
                        pArc.Radius = double.Parse(groupCode.Value);
                        break;
                    case "50":
                        pArc.StartRadian = double.Parse(groupCode.Value);
                        break;
                    case "51":
                        pArc.EndRadian = double.Parse(groupCode.Value);
                        break;
                }
                groupCode = ReadGroupCode();
            }

            //DXF中数据为角度，且起算边、方向与正常情况一致，故只需转换为弧度即可
            pArc.StartRadian = pArc.StartRadian * (Math.PI / 180);
            pArc.EndRadian = pArc.EndRadian * (Math.PI / 180);

            //DXF对于360度会记录为0，所以在终止弧度小于起始弧度时应该+2PI
            if (pArc.EndRadian < pArc.StartRadian)
                pArc.EndRadian = pArc.EndRadian + Math.PI * 2;

       

            return pArc;
        }

        private EllipseClass readEllipse()
        {
            EllipseClass pEllipse = new EllipseClass();

            groupCode = ReadGroupCode();
            while (groupCode.Code != "0") //以下一个图元开始符0为结束标志
            {
                switch (groupCode.Code)
                {
                    case "8":
                        pEllipse.LayerName = groupCode.Value;
                        break;
                    case "10":
                        pEllipse.Centre.X = double.Parse(groupCode.Value);
                        break;
                    case "20":
                        pEllipse.Centre.Y = double.Parse(groupCode.Value);
                        break;
                    case "30":
                        pEllipse.Centre.Z = double.Parse(groupCode.Value);
                        break;
                    case "11":
                        pEllipse.MajorRadiusVertex.X = double.Parse(groupCode.Value);
                        break;
                    case "21":
                        pEllipse.MajorRadiusVertex.Y = double.Parse(groupCode.Value);
                        break;
                    case "31":
                        pEllipse.MajorRadiusVertex.Z = double.Parse(groupCode.Value);
                        break;
                    case "40":
                        pEllipse.RadiusRatio = double.Parse(groupCode.Value);
                        break;
                    case "41":
                        pEllipse.StartRadian = double.Parse(groupCode.Value);
                        break;
                    case "42":
                        pEllipse.EndRadian = double.Parse(groupCode.Value);
                        break;
                }
                groupCode = ReadGroupCode();
            }


            //计算长轴长度
            //由于DXF不直接记录长短轴，但可以根据长轴顶点到中心点的距离计算，而DXF记录的是相对于中心点的坐标，故长轴距离可直接由其坐标计算得出
            pEllipse.MajorRadius = (double)Math.Sqrt(
                Math.Pow(pEllipse.MajorRadiusVertex.X, 2) +
                Math.Pow(pEllipse.MajorRadiusVertex.Y, 2) +
                Math.Pow(pEllipse.MajorRadiusVertex.Z, 2));

            //计算短轴长度
            //根据长短轴比例计算短轴长度
            pEllipse.MinorRadius = pEllipse.MajorRadius * pEllipse.RadiusRatio;


            //计算椭圆默认角度起算线与X轴正向夹角
            //默认角度即为长轴顶点向量
            if (Math.Round(pEllipse.EndRadian - pEllipse.StartRadian, 6) != 6.283185)
            {
                //只处理非闭合的椭圆弧
                double angleDiff;
                Vector vector_MajorRadiusVertex = new Vector(pEllipse.MajorRadiusVertex.X, pEllipse.MajorRadiusVertex.Y, pEllipse.MajorRadiusVertex.Z);
                Vector vector_Axis = new Vector(1, 0, 0);
                if (vector_MajorRadiusVertex.Y >= 0)
                {
                    //如果长轴顶点位于Y轴上方，则角度差值直接计算
                    angleDiff = VectorHelper.Calc_AngleInRadian(vector_MajorRadiusVertex, vector_Axis);
                }
                else
                {
                    //如果长轴顶点位于Y轴下方，则角度差值应该是计算结果的补角
                    angleDiff = (double)Math.PI * 2 - VectorHelper.Calc_AngleInRadian(vector_MajorRadiusVertex, vector_Axis);
                }

                //根据角度差值重新计算起始点角度
                pEllipse.StartRadian = (pEllipse.StartRadian + angleDiff);
                pEllipse.EndRadian = (pEllipse.EndRadian + angleDiff);
                if (pEllipse.StartRadian > Math.PI * 2)
                {
                    int ratio = (int)(pEllipse.StartRadian / (Math.PI * 2));
                    pEllipse.StartRadian = pEllipse.StartRadian - ratio * (double)Math.PI * 2;
                    pEllipse.EndRadian = pEllipse.EndRadian - ratio * (double)Math.PI * 2;
                }

            }



            //将长轴相对坐标换算为绝对坐标
            //由于组码11、21、31记录的是长轴正向顶点相对于中心点的坐标，故需转换为绝对坐标
            pEllipse.MajorRadiusVertex.X = pEllipse.MajorRadiusVertex.X + pEllipse.Centre.X;
            pEllipse.MajorRadiusVertex.Y = pEllipse.MajorRadiusVertex.Y + pEllipse.Centre.Y;
            pEllipse.MajorRadiusVertex.Z = pEllipse.MajorRadiusVertex.Z + pEllipse.Centre.Z;

            return pEllipse;
        }

        private InsertClass readInsert()
        {
            InsertClass pInsert = new InsertClass();

            groupCode = ReadGroupCode();
            while (groupCode.Code != "0") //以下一个图元开始符0为结束标志
            {
                switch (groupCode.Code)
                {
                    case "8":
                        pInsert.LayerName = groupCode.Value;
                        break;
                    case "2":
                        pInsert.BlockName = groupCode.Value;
                        break;
                    case "10":
                        pInsert.InsertPoint.X = double.Parse(groupCode.Value);
                        break;
                    case "20":
                        pInsert.InsertPoint.Y = double.Parse(groupCode.Value);
                        break;
                    case "30":
                        pInsert.InsertPoint.Z = double.Parse(groupCode.Value);
                        break;
                    case "41":
                        pInsert.ScaleX = double.Parse(groupCode.Value);
                        break;
                    case "42":
                        pInsert.ScaleY = double.Parse(groupCode.Value);
                        break;
                    case "43":
                        pInsert.ScaleZ = double.Parse(groupCode.Value);
                        break;
                    case "50":
                        pInsert.RotationAngle = double.Parse(groupCode.Value);
                        break;
                }
                groupCode = ReadGroupCode();

            }
            return pInsert;

        }

        private TextClass readText()
        {
            TextClass pText = new TextClass();

            groupCode = ReadGroupCode();
            while (groupCode.Code != "0") //以下一个图元开始符0为结束标志
            {
                switch (groupCode.Code)
                {
                    case "8":
                        pText.LayerName = groupCode.Value;
                        break;
                    case "10":
                        pText.BasePoint.X = double.Parse(groupCode.Value);
                        break;
                    case "20":
                        pText.BasePoint.Y = double.Parse(groupCode.Value);
                        break;
                    case "30":
                        pText.BasePoint.Z = double.Parse(groupCode.Value);
                        break;
                    case "1":
                        //由于多行文字中的换行符在DXF文件中以/P表示，故需将此替换成空字符串
                        pText.Content= groupCode.Value.Replace("\\P",string.Empty);
                        break;
                }
                groupCode = ReadGroupCode();

            }
            return pText;
        }
        #endregion

        private GroupCode ReadGroupCode()
        {
            GroupCode groupCode = new GroupCode();
            groupCode.Code = streamReader.ReadLine().Trim();
            groupCode.Value = streamReader.ReadLine().Trim();

            return groupCode;
        }

        private void MergeLines()
        {
            if (LwPolylines != null)
            {
                foreach (var item in LwPolylines)
                {
                    //IEnumerable<LineClass> pLines = item.LineCollection;
                    Lines.AddRange(item.LineCollection as IEnumerable<LineClass>);
                }
            }
        }
        #endregion
    }
}
