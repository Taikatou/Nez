﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Nez.UI;


#if DEBUG
namespace Nez
{
	/// <summary>
	/// the heart of the inspector system. Subclasses of Inspector are responsible for setting up and managing the UI. Currently,
	/// custom type handling is not yet implemented.
	/// </summary>
	public abstract class Inspector
	{
		protected object _target;
		protected string _name;
		protected Type _valueType;
		protected Func<object> _getter;
		protected Action<object> _setter;
		protected MemberInfo _fieldOrPropInfo;


		public static List<Inspector> getInspectableProperties( object component )
		{
			var props = new List<Inspector>();
			var componentType = component.GetType();

			var fields = ReflectionUtils.getFields( componentType );
			foreach( var field in fields )
			{
				if( !field.IsPublic && field.GetCustomAttribute<InspectableAttribute>() == null )
					continue;

				var inspector = getInspectorForType( field.FieldType );
				if( inspector != null )
				{
					inspector.setTarget( component, field );
					props.Add( inspector );
				}
			}

			var properties = ReflectionUtils.getProperties( componentType );
			foreach( var prop in properties )
			{
				if( !prop.CanRead || !prop.CanWrite )
					continue;

				if( ( !prop.GetMethod.IsPublic || !prop.SetMethod.IsPublic ) && prop.GetCustomAttribute<InspectableAttribute>() == null )
					continue;

				var inspector = getInspectorForType( prop.PropertyType );
				if( inspector != null )
				{
					inspector.setTarget( component, prop );
					props.Add( inspector );
				}
			}

			return props;
		}


		public static List<Inspector> getTransformProperties( object transform )
		{
			var props = new List<Inspector>();
			var type = transform.GetType();

			var allowedProps = new string[] { "localPosition", "localRotationDegrees", "localScale" };
			var properties = ReflectionUtils.getProperties( type );
			foreach( var prop in properties )
			{
				if( !allowedProps.contains( prop.Name ) )
					continue;

				var inspector = getInspectorForType( prop.PropertyType );
				inspector.setTarget( transform, prop );
				props.Add( inspector );
			}

			return props;
		}


		/// <summary>
		/// gets an Inspector subclass that can handle valueType. If no default Inspector is available the memberInfo custom attributes
		/// will be checked for the CustomInspectorAttribute.
		/// </summary>
		/// <returns>The inspector for type.</returns>
		/// <param name="valueType">Value type.</param>
		/// <param name="memberInfo">Member info.</param>
		static Inspector getInspectorForType( Type valueType )
		{
			if( valueType == typeof( int ) )
				return new IntInspector();
			if( valueType == typeof( float ) )
				return new FloatInspector();
			if( valueType == typeof( bool ) )
				return new BoolInspector();
			if( valueType == typeof( string ) )
				return new StringInspector();
			if( valueType == typeof( Vector2 ) )
				return new Vector2Inspector();

			var customInspectorType = valueType.GetTypeInfo().GetCustomAttribute<CustomInspectorAttribute>();
			if( customInspectorType != null )
			{
				if( customInspectorType.inspectorType.GetTypeInfo().IsSubclassOf( typeof( Inspector ) ) )
					return (Inspector)Activator.CreateInstance( customInspectorType.inspectorType );
				Debug.log( $"found CustomInspector {customInspectorType.inspectorType} but it is not a subclass of Inspector" );
			}

			Debug.log( $"no inspector for type {valueType}" );

			return null;
		}


		public void setTarget( object target, FieldInfo field )
		{
			_fieldOrPropInfo = field;
			_target = target;
			_name = field.Name;
			_valueType = field.FieldType;

			_getter = () =>
			{
				return field.GetValue( target );
			};
			_setter = ( val ) =>
			{
				field.SetValue( target, val );
			};
		}


		public void setTarget( object target, PropertyInfo prop )
		{
			_fieldOrPropInfo = prop;
			_target = target;
			_name = prop.Name;
			_valueType = prop.PropertyType;

			_getter = () =>
			{
				return ReflectionUtils.getPropertyGetter( prop ).Invoke( target, null );
			};
			_setter = ( val ) =>
			{
				ReflectionUtils.getPropertySetter( prop ).Invoke( target, new object[] { val } );
			};
		}


		protected T getValue<T>()
		{
			return (T)_getter.Invoke();
		}


		protected void setValue( object value )
		{
			_setter.Invoke( value );
		}


		protected T getFieldOrPropertyAttribute<T>() where T : Attribute
		{
			return _fieldOrPropInfo.GetCustomAttribute<T>();
		}


		/// <summary>
		/// used to setup the UI for the Inspector
		/// </summary>
		/// <param name="table">Table.</param>
		/// <param name="skin">Skin.</param>
		public abstract void initialize( Table table, Skin skin );


		/// <summary>
		/// used to update the UI for the Inspector
		/// </summary>
		public abstract void update();

	}
}
#endif