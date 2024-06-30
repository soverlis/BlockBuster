using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlockBusterApp
{
    public partial class Form1 : Form
    {

        private PointF[] polygonVertices;
        private float circleX = 250f, circleY = 250f, circleRadius = 20f;

        private List<Rectangle> immovableRectangles;
        private float circleMoveStepX = 4f, circleMoveStepY = 4f;

        private Timer timer;

        private PictureBox pictureBox;

        private int bottomTouchCount = 0;

        private float rotationAngle = 0.0f;


        public Form1()
        {
            pictureBox = new PictureBox();
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(1500, 900);
            pictureBox.Location = new Point(450, 250);
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Controls.Add(pictureBox);
            pictureBox.Paint += new PaintEventHandler(PictureBox_Paint);

            this.KeyDown += new KeyEventHandler(PictureBox_KeyDown);
            this.KeyPreview = true;

            polygonVertices = new PointF[]
            {
                new PointF(750, 750),
                new PointF(750, 820),
                new PointF(900, 820),
                new PointF(900, 750)
            };
            this.DoubleBuffered = true;

            InitializeImmovableRectangles();

            timer = new Timer();
            timer.Interval = 50;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();



            this.Controls.Add(pictureBox);
        }
        private void timer_Tick(object sender, EventArgs e)
        {

            circleX += circleMoveStepX;
            circleY += circleMoveStepY;


            CheckPictureBoxBounds();
            
            BounceCircleOffRotatedPolygon();

            CheckCircleImmovableRectangleCollisions();

            pictureBox.Invalidate();
        }
        private void CheckPictureBoxBounds()
        {
            if (circleX - circleRadius < 0 || circleX + circleRadius > pictureBox.Width)
            {
                circleMoveStepX = - circleMoveStepX;
            }
            if (circleY - circleRadius < 0)
            {
                circleMoveStepY = - circleMoveStepY;
            }
            if (circleY + circleRadius > pictureBox.Height)
            {
                bottomTouchCount++;
                if (bottomTouchCount >= 3)
                {
                    circleMoveStepX = 0;
                    circleMoveStepY = 0;
                }
                else
                {
                    circleX = 250f;
                    circleY = 250f;
                    circleMoveStepX = 4f;
                    circleMoveStepY = 4f;
                }
            }
        }
        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Matrix matrix = new Matrix();
            matrix.RotateAt(rotationAngle, GetPolygonCentroid(polygonVertices));
            Pen blackPen = new Pen(Color.Black, 3);
            SolidBrush redBrush = new SolidBrush(Color.Red);
            SolidBrush blueBrush = new SolidBrush(Color.Cyan);
            SolidBrush greenBrush = new SolidBrush(Color.Green);

            g.Transform = matrix;


            g.DrawPolygon(blackPen, polygonVertices);
            g.FillPolygon(redBrush, polygonVertices);

            g.ResetTransform();

            g.DrawEllipse(blackPen, circleX - circleRadius, circleY - circleRadius, 2 * circleRadius, 2 * circleRadius);
            g.FillEllipse(greenBrush, circleX - circleRadius, circleY - circleRadius, 2 * circleRadius, 2 * circleRadius);

            foreach (var immovableRect in immovableRectangles)
            {
                g.DrawRectangle(blackPen, immovableRect);
                g.FillRectangle(blueBrush, immovableRect);
            }
        }
        private void PictureBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                MovePolygon(-10, 0);

            }
            else if (e.KeyCode == Keys.Right)
            {
                MovePolygon(10, 0);
            }
            else if (e.KeyCode == Keys.Up)
            {
                rotationAngle -= 10f;
            }
            else if (e.KeyCode == Keys.Down)
            {
                rotationAngle += 10f;
            }
            pictureBox.Invalidate();
        }

        private void MovePolygon(float deltaX, float deltaY)
        {
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                polygonVertices[i].X += deltaX;
                polygonVertices[i].Y += deltaY;
            }
        }

        private PointF GetPolygonCentroid(PointF[] vertices)
        {
            float centroidX = 0, centroidY = 0;
            foreach (PointF vertex in vertices)
            {
                centroidX += vertex.X;
                centroidY += vertex.Y;
            }
            centroidX /= vertices.Length;
            centroidY /= vertices.Length;
            return new PointF(centroidX, centroidY);
        }

        private void InitializeImmovableRectangles()
        {
            immovableRectangles = new List<Rectangle>
            {
                new Rectangle(0, 100, 150, 75),
                new Rectangle(155, 100, 150, 75),
                new Rectangle(310, 100, 150, 75),
                new Rectangle(465, 100, 150, 75),
                new Rectangle(620, 100, 150, 75),
                new Rectangle(775, 100, 150, 75),
                new Rectangle(930, 100, 150, 75),
                new Rectangle(1085, 100, 150, 75),
                new Rectangle(1240, 100, 150, 75)

            };
        }

        private void CheckCircleImmovableRectangleCollisions()
        {
            for (int i = immovableRectangles.Count - 1; i >= 0; i--)
            {
                if (IsCircleIntersectingRectangle(circleX, circleY, circleRadius, immovableRectangles[i]))
                {
                    immovableRectangles.RemoveAt(i);
                    circleMoveStepX = -circleMoveStepX;
                    circleMoveStepY = -circleMoveStepY;
                }
            }

            if (immovableRectangles.Count == 0)
            {
                InitializeImmovableRectangles();
            }
        }
        private bool IsCircleIntersectingRectangle(float circleX, float circleY, float radius, RectangleF rect)
        {
            return circleX + radius > rect.Left && circleX - radius < rect.Right &&
                   circleY + radius > rect.Top && circleY - radius < rect.Bottom;
        }
        private void BounceCircleOffRotatedPolygon()
        {
            var rotatedVertices = GetRotatedPolygonVertices();
            for (int i = 0; i < rotatedVertices.Length; i++)
            {
                PointF p1 = rotatedVertices[i];
                PointF p2 = rotatedVertices[(i + 1) % rotatedVertices.Length];
                if (IsCircleIntersectingEdge(circleX, circleY, circleRadius, p1, p2))
                {
                    float dx = p2.X - p1.X;
                    float dy = p2.Y - p1.Y;
                    float length = (float)Math.Sqrt(dx * dx + dy * dy);
                    float nx = -dy / length;
                    float ny = dx / length;

                    float dotProduct = circleMoveStepX * nx + circleMoveStepY * ny;
                    circleMoveStepX = (circleMoveStepX - 2 * dotProduct * nx);
                    circleMoveStepY = (circleMoveStepY - 2 * dotProduct * ny);
                    return;

                }
            }
        }
        
        private bool IsCircleIntersectingEdge(float cx, float cy, float radius, PointF p1, PointF p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float nx = -dy / length;
            float ny = dx / length;
            float ex = dx / length;
            float ey = dy / length;

            float vx = cx - p1.X;
            float vy = cy - p1.Y;
            float dotProduct = vx * nx + vy * ny;
            float dotProduct2 = vx * ex + vy * ey;

            return Math.Abs(dotProduct) <= radius;
        }
        private PointF[] GetRotatedPolygonVertices()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(polygonVertices);

            Matrix matrix = new Matrix();
            matrix.RotateAt(rotationAngle, GetPolygonCentroid(polygonVertices));
            path.Transform(matrix);

            return path.PathPoints;
        }

    }
}
