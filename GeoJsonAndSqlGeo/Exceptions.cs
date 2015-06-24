using System;
using Irony;

namespace GeoJsonToSqlServer
{
    public class NoSpatialReferenceDefinedException : Exception
    {
        public NoSpatialReferenceDefinedException() : base("No spatial reference system has been set. Set one via GeoToSql.SetReferenceSystem(SpatialReferenceSystem)")
        {
            
        }
    }

    public class SqlGeometryParseException : Exception
    {
        internal SqlGeometryParseException(Irony.Parsing.ParseTree tree)
            : this(tree.ParserMessages[0])
        {
            ParseTree = tree;
        }

        private SqlGeometryParseException(LogMessage mg) : base(string.Format("{0}, ({1}:{2})", mg.Message, mg.Location.Line, mg.Location.Column))
        {
            
        }

        /// <summary>
        /// The contained object is of type Irony.Parsing.ParseTree
        /// </summary>
        public object ParseTree
        {
            get; private set;
        }
    }
}