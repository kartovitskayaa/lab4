using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RendererConsole
{
    public class Camera
    {
        private Vertex position;
        private Vertex direction;

        public Vertex Position { get => position; set => position = value; }

        public Vertex Direction { get => direction; set => direction = value; }

        public Vertex Up { get; set; }

        public void MoveLeft(double delta)
        {
            this.position = this.position.Add(this.direction.CrossProduct(this.Up).Normalize().Multily(-delta));
        }

        public void MoveRight(double delta)
        {
            this.position = this.position.Add(this.direction.CrossProduct(this.Up).Normalize().Multily(delta));
        }

        public void MoveFront(double delta)
        {
            this.position = this.position.Add(this.direction.Normalize().Multily(delta));
        }

        public void MoveBack(double delta)
        {
            this.position = this.position.Add(this.direction.Normalize().Multily(-delta));
        }

        public double[,] GetLookAtMatrix()
        {
            Vertex axisZ = Position.Substract(Direction).Normalize();
            Vertex axisY = new Vertex(0, 1, 0);
            Vertex axisX = axisY.CrossProduct(axisZ).Normalize();

            return new double[,]
            {
                { axisX.X, axisY.X, axisZ.X, 0 },
                { axisX.Y, axisY.Y, axisZ.Y, 0 },
                { axisX.Z, axisY.Z, axisZ.Z, 0 },
                { -axisX.DotProduct(Position), -axisY.DotProduct(Position), -axisZ.DotProduct(Position), 1 },
            };
        }
    }
}
