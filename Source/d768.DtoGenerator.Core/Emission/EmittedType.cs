using System;
using System.Linq;
using System.Reflection.Emit;
using d768.DtoGenerator.Core.Infrastructure;
using LanguageExt;
using Mono.Cecil;
using FieldAttributes = System.Reflection.FieldAttributes;
using MethodAttributes = System.Reflection.MethodAttributes;
using PropertyAttributes = System.Reflection.PropertyAttributes;
using TypeAttributes = System.Reflection.TypeAttributes;

namespace d768.DtoGenerator.Core.Emission
{
    public class EmittedType
    {
        private readonly EmittedTypeDefinition _typeDefinition;
        private readonly ModuleBuilder _moduleBuilder;

        public EmittedType(EmittedTypeDefinition typeDefinition, ModuleBuilder moduleBuilder)
        {
            _typeDefinition = typeDefinition;
            _moduleBuilder = moduleBuilder;
        }

        public Either<EmissionError, Type> CreateType()
            => CreateTypeInternal();

        private Either<EmissionError, Type> CreateTypeInternal()
        {
            TypeBuilder tb = _moduleBuilder.DefineType(
                _typeDefinition.Name,
                TypeAttributes.Public
                | TypeAttributes.Class);

            tb
                .DefineDefaultConstructor(
                    MethodAttributes.Public
                    | MethodAttributes.SpecialName
                    | MethodAttributes.RTSpecialName);


            var propertiesCreationResult = _typeDefinition
                .Properties
                .Select(x => CreateProperty(tb, x))
                .FirstOrDefault(x => x.IsSome);

            if (propertiesCreationResult != null && propertiesCreationResult.IsSome)
                return (EmissionError) propertiesCreationResult;

            return tb.CreateType();
        }

        private Option<EmissionError> CreateProperty(
            TypeBuilder typeBuilder,
            EmittedTypeDefinition property)
        {
            if (property.IsCollection)
            {
                return CreateCollectionProperty(typeBuilder, property);
            } 
            else if (property.IsPrimitive || !property.IsAnonymous)
            {
                return CreateRegularProperty(typeBuilder, property);
            }
            else
            {
                return CreateAnonymousProperty(typeBuilder, property);
            }
        }

        private Option<EmissionError> CreateAnonymousProperty(TypeBuilder typeBuilder,
            EmittedTypeDefinition property)
        {
            var typeResult = new EmittedType(new EmittedTypeDefinition(
                    _typeDefinition.Name + property.Name, property.Type), _moduleBuilder)
                .CreateType();

            if (typeResult.IsLeft)
            {
                return (EmissionError)typeResult;
            }

            var propertyType = (Type)typeResult;
            
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + property.Name,
                propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder =
                typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault,
                    propertyType, null);
            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + property.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + property.Name,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig,
                null, new[] {propertyType});

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);

            return Option<EmissionError>.None;
        }

        private Option<EmissionError> CreateCollectionProperty(TypeBuilder typeBuilder,
            EmittedTypeDefinition property)
        {
            var genericType = property.Type as GenericInstanceType;
            
            
            return Option<EmissionError>.None;
        }

        private Option<EmissionError> CreateRegularProperty(TypeBuilder typeBuilder,
            EmittedTypeDefinition property)
        {
            Type propertyType = Type.GetType(
                property.Type.FullName + ", " + property.Type.Module.Assembly.FullName);

            if (propertyType == null)
            {
                
            } 
            
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + property.Name,
                propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder =
                typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault,
                    propertyType, null);
            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + property.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + property.Name,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig,
                null, new[] {propertyType});

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);

            return Option<EmissionError>.None;
        }
    }
}