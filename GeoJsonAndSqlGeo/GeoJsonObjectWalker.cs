using System;
using System.Collections.Generic;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace GeoJsonAndSqlGeo
{
    /// <summary>
    /// Class that helps you visit some hierarchy of geometry objects.
    /// The GeoJson.NET library is somewhat special in that you have GeoJson abstract object
    /// and a IGeographyObject interface. All concrete items inherit from the abstract class,
    /// but only the geography items inherit the interface.
    /// </summary>
    public class GeoJsonObjectWalker
    {
        private readonly GeoJSONObject _geoJsonObject;

        public GeoJsonObjectWalker(GeoJSONObject geoJsonObject)
        {
            _geoJsonObject = geoJsonObject;
        }

        /// <summary>
        /// walks the visitor through the geo json object
        /// </summary>
        public void CarryOut(IGeoJsonObjectVisitor visitor)
        {
            AcceptVisitor(new GeoWalkContext<GeoJSONObject>(_geoJsonObject), visitor);
        }

        private void AcceptVisitor(IGeoWalkContext obj, IGeoJsonObjectVisitor visitor)
        {
            IfIsObjDo<Point>(obj, visitor.Visit);
            IfIsObjDo<MultiPoint>(obj, m =>
            {
                visitor.Visit(m);
                DispatchVisitor(m, m.Item.Coordinates, visitor);
                m.CallExitActivity();
            });
            IfIsObjDo<LineString>(obj, visitor.Visit);
            IfIsObjDo<MultiLineString>(obj, mls =>
            {
                visitor.Visit(mls);
                DispatchVisitor(mls, mls.Item.Coordinates, visitor);
                mls.CallExitActivity();
            });
            IfIsObjDo<Polygon>(obj, pl =>
            {
                visitor.Visit(pl);
                DispatchVisitor(pl, pl.Item.Coordinates, visitor);
                pl.CallExitActivity();
            });
            IfIsObjDo<MultiPolygon>(obj, mpl =>
            {
                visitor.Visit(mpl);
                DispatchVisitor(mpl, mpl.Item.Coordinates, visitor);
                mpl.CallExitActivity();
            });
            IfIsObjDo<GeometryCollection>(obj, gcl =>
            {
                visitor.Visit(gcl);
                DispatchVisitor(gcl, gcl.Item.Geometries, visitor);
                gcl.CallExitActivity();
            });

            IfIsObjDo<FeatureCollection>(obj, fcl =>
            {
                visitor.Visit(fcl);
                DispatchVisitor(fcl, fcl.Item.Features, visitor);
                fcl.CallExitActivity();
            });

            IfIsObjDo<Feature>(obj, f =>
            {
                visitor.Visit(f);
                AcceptVisitor(f.SpawnForChild((GeoJSONObject)f.Item.Geometry), visitor);
                f.CallExitActivity();
            });
        }

        private void DispatchVisitor<T>(IGeoWalkContext currentContext, IEnumerable<T> items, IGeoJsonObjectVisitor visitor)
        {
            foreach (var geoObj in items)
                AcceptVisitor(currentContext.SpawnForChild((GeoJSONObject)(object)geoObj), visitor);
        }

        private static void IfIsObjDo<T>(IGeoWalkContext obj, Action<GeoWalkContext<T>> actionWithItem) where T : GeoJSONObject
        {
            if (obj.Item is T)
                actionWithItem(obj.Repurpose<T>());
        }
    }

    /// <summary>
    /// Implement to be able to visit a geojson geometry object
    /// </summary>
    public interface IGeoJsonObjectVisitor
    {
        void Visit(GeoWalkContext<Point> p);
        void Visit(GeoWalkContext<MultiPoint> mp);
        void Visit(GeoWalkContext<LineString> ls);
        void Visit(GeoWalkContext<MultiLineString> mls);
        void Visit(GeoWalkContext<Polygon> pl);
        void Visit(GeoWalkContext<MultiPolygon> mpl);
        void Visit(GeoWalkContext<GeometryCollection> gcl);
        void Visit(GeoWalkContext<Feature> f);
        void Visit(GeoWalkContext<FeatureCollection> fcl);
    }

    /// <summary>
    /// Gives access to the current context of a GeoJson object walk
    /// </summary>
    public class GeoWalkContext<T> : IGeoWalkContext where T : GeoJSONObject
    {
        private Action _exitAction = () => {};

        public GeoWalkContext(T item) : this(item, 0, null)
        {
        }

        private GeoWalkContext(T item, int depth, IGeoWalkContext parent)
        {
            CurrentDepth = depth;
            Item = item;
            Parent = parent;
        }

        public int CurrentDepth { get; private set; }

        public T Item { get; private set; }

        public IGeoWalkContext Parent { get; private set; }

        GeoJSONObject IGeoWalkContext.Item
        {
            get { return Item; }
        }

        public GeoWalkContext<Z> SpawnForChild<Z>(Z item) where Z : GeoJSONObject
        {
            return new GeoWalkContext<Z>(item, CurrentDepth + 1, this);
        }

        public GeoWalkContext<Z> Repurpose<Z>() where Z : GeoJSONObject
        {
            if (typeof (T) == typeof (Z))
                return (GeoWalkContext<Z>)(object)this; // ARGH
            return new GeoWalkContext<Z>((Z)(GeoJSONObject)Item, CurrentDepth, Parent);
        }

        internal void CallExitActivity()
        {
            _exitAction();
        }

        public void SetExitActivity(Action a)
        {
            _exitAction += a;
        }
    }

    public static class GeoWalkerExtensions
    {
        /// <summary>
        /// Check if there is a parent and if its item is of type T.
        /// Returns true if those conditions are met.
        /// </summary>
        public static bool HasParentCorrespondingTo<T>(this IGeoWalkContext ctx)
        {
            return ctx.Parent != null && ctx.Parent.Item is T;
        }
    }

    public interface IGeoWalkContext
    {
        int CurrentDepth { get; }
        /// <summary>
        /// Access to the parent's geo walk context. MAy be null for the root geo object.
        /// </summary>
        IGeoWalkContext Parent { get; }

        /// <summary>
        /// Current Item associated with this context instance
        /// </summary>
        GeoJSONObject Item { get; }

        /// <summary>
        /// creates a new context, intended to be used for childs of a given parent
        /// </summary>
        GeoWalkContext<T> SpawnForChild<T>(T item) where T : GeoJSONObject;

        /// <summary>
        /// Provides access to this context as a closed generic obejct
        /// </summary>
        GeoWalkContext<T> Repurpose<T>() where T : GeoJSONObject;

        /// <summary>
        /// Sets an exit activity that is called when the current context leaves the focus of a current walk
        /// </summary>
        void SetExitActivity(Action a);
    }

    /// <summary>
    /// Basic implementation that allows you to override only what you need
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public abstract class GeoJsonObjectVisitor : IGeoJsonObjectVisitor
    {
        public virtual void Visit(GeoWalkContext<Point> p) { }

        public virtual void Visit(GeoWalkContext<MultiPoint> mp) { }

        public virtual void Visit(GeoWalkContext<LineString> ls) { }

        public virtual void Visit(GeoWalkContext<MultiLineString> mls) { }

        public virtual void Visit(GeoWalkContext<Polygon> pl) { }

        public virtual void Visit(GeoWalkContext<MultiPolygon> mpl) { }

        public virtual void Visit(GeoWalkContext<GeometryCollection> gcl) { }
        
        public virtual void Visit(GeoWalkContext<Feature> f) { }
        
        public virtual void Visit(GeoWalkContext<FeatureCollection> fcl) { }
    }
}