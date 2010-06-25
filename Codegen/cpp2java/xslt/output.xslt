<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xd="http://www.pnp-software.com/XSLTdoc" xmlns:exsl="http://exslt.org/common" extension-element-prefixes="exsl" version="2.0">
	<!-- imports.xslt -->
	<xsl:import href="imports.xslt"/>
	<xsl:output method="text" name="textOutput"/>
	<!-- auxiliary variable for simple types -->
	<xsl:variable name="simpleTypes" select="'boolean char byte short int long java.math.BigInteger float double String Real'"/>
	<xsl:variable name="vectors" select="'Vector2 Vector3 Vector4'"/>
	<!-- config.xml -->
	<xsl:variable name="config" select="document('config.xml')"/>
	<xsl:variable name="path" select="$config/config/path"/>
	<xsl:variable name="ignore" select="document('ignorelist.xml')"/>
	<!-- node-set of typedef simple types (e.g. <mapping type="RGBA">long</mapping>) -->
	<xsl:variable name="typedefResultTree">
		<xsl:for-each select="meta/namespace/typedef">
			<xsl:variable name="originalName" select="@name"/>
			<xsl:call-template name="typedefMapping">
				<xsl:with-param name="name" select="@name"/>
				<xsl:with-param name="originalName" select="@name"/>
			</xsl:call-template>
		</xsl:for-each>
	</xsl:variable>
	<xsl:template match="/">
		<xsl:apply-imports/>
		<!-- ############################### CONSTANTS, global enums and global functions ################################# -->
		<xsl:variable name="constants" select="$config/config/constants"/>
		<xsl:for-each select="meta/namespace">
			<xsl:if test="enumeration or function or variable">
				<xsl:variable name="filename" select="concat($path,'/src/java/org/',@name,'4j/',@second,$constants,'.java')"/>
				<xsl:result-document href="{$filename}" format="textOutput">
					<xsl:text>package org.ogre4j;&#10;&#10;</xsl:text>
					<xsl:call-template name="imports"/>
					<!-- debug start
					<xsl:for-each select="exsl:node-set($typedefResultTree)/mapping">
						<xsl:text>type:	</xsl:text>
						<xsl:value-of select="@type"/>
						<xsl:text>		value:	</xsl:text>
						<xsl:value-of select="."/>
						<xsl:text>&#10;</xsl:text>
					</xsl:for-each>
					<xsl:text>&#10;</xsl:text>
					<xsl:text>&#10;</xsl:text>
					<xsl:text>Ignorelist</xsl:text>
					<xsl:text>&#10;</xsl:text>
					<xsl:value-of select="$ignore/ignorelist/ignore"/>
					<xsl:text>&#10;</xsl:text>
					<xsl:text>&#10;</xsl:text>
					 debug end -->
					<xsl:text>public class </xsl:text>
					<xsl:value-of select="$constants"/>
					<xsl:text> extends NativeObject {&#10;</xsl:text>
					<!-- global typedefs -->
					<!--<xsl:for-each select="typedef">
						<xsl:call-template name="typedef"/>
					</xsl:for-each>-->
					<!-- global enumerations -->
					<xsl:for-each select="enumeration">
						<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
							<xsl:call-template name="enumeration"/>
						</xsl:if>
					</xsl:for-each>
					<!-- global functions -->
					<xsl:for-each select="function">
						<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
							<xsl:call-template name="function"/>
							<xsl:call-template name="nativeFunction"/>
						</xsl:if>
					</xsl:for-each>
					<!-- global variables -->
					<xsl:for-each select="variable">
						<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
							<xsl:text>	public </xsl:text>
							<xsl:value-of select="type"/>
							<xsl:text> </xsl:text>
							<xsl:value-of select="name"/>
							<xsl:text>;&#10;</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>	public void dispose() { }&#10;</xsl:text>
					<xsl:text>}</xsl:text>
				</xsl:result-document>
			</xsl:if>
		</xsl:for-each>
		<!-- ############################### GLOBAL TYPEDEFS ################################# -->
		<xsl:for-each select="meta/namespace/typedef">
			<!-- no simple types and no ingnores -->
			<xsl:if test="not(@name=exsl:node-set($typedefResultTree)/mapping/@type) and not(@name=$ignore/ignorelist/ignore)">
				<xsl:variable name="filename" select="concat($path,'/src/java/org/ogre4j/',@name,'.java')"/>
				<!--<xsl:variable name="currentClass" select="@name"/>-->
				<xsl:result-document href="{$filename}" format="textOutput">
					<xsl:text>package org.ogre4j;&#10;&#10;</xsl:text>
					<xsl:call-template name="imports">
						<xsl:with-param name="calledFrom" select="'globalTypedef'"/>
					</xsl:call-template>
					<xsl:call-template name="typedef"/>
				</xsl:result-document>
			</xsl:if>
		</xsl:for-each>
		<!-- ############################### Ogre4jEnum.cpp ################################# -->
		<xsl:variable name="prefix" select="$config/config/prefix_cpp"/>
		<xsl:variable name="filename" select="concat($path,'/src/native/src/',$prefix,'_Enum.cpp')"/>
		<xsl:result-document href="{$filename}" format="textOutput">
			<xsl:for-each select="root()//enumeration">
				<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
					<xsl:choose>
						<xsl:when test="starts-with(@name,'@')">
							<xsl:if test="not(preceding-sibling::node()[starts-with(@name,'@')])">
								<xsl:variable name="enumResultTree">
									<xsl:for-each select="../enumeration[starts-with(@name,'@')]/enum">
										<xsl:element name="enum">
											<xsl:attribute name="name" select="@name"/>
										</xsl:element>
									</xsl:for-each>
								</xsl:variable>
								<xsl:call-template name="cEnumeration">
									<xsl:with-param name="enumList" select="$enumResultTree"/>
								</xsl:call-template>
							</xsl:if>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="cEnumeration"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</xsl:for-each>
		</xsl:result-document>
		<!-- ############################### Ogre4jEnum.h ################################# -->
		<xsl:variable name="prefix" select="$config/config/prefix_cpp"/>
		<xsl:variable name="filename" select="concat($path,'/src/native/include/',$prefix,'Enum.h')"/>
		<xsl:result-document href="{$filename}" format="textOutput">
			<xsl:text>#ifndef _OGRE4J_ENUM_H_&#10;</xsl:text>
			<xsl:text>#define _OGRE4J_ENUM_H_&#10;</xsl:text>
			<xsl:text>&#10;</xsl:text>
			<xsl:text>namespace Ogre4J {&#10;</xsl:text>
			<xsl:text>&#10;</xsl:text>
			<xsl:for-each select="root()//enumeration">
				<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
					<xsl:choose>
						<xsl:when test="starts-with(@name,'@')">
							<xsl:if test="not(preceding-sibling::node()[starts-with(@name,'@')])">
								<xsl:variable name="enumResultTree">
									<xsl:for-each select="../enumeration[starts-with(@name,'@')]/enum">
										<xsl:element name="enum">
											<xsl:attribute name="name" select="@name"/>
										</xsl:element>
									</xsl:for-each>
								</xsl:variable>
								<xsl:call-template name="cEnumeration">
									<xsl:with-param name="enumList" select="$enumResultTree"/>
									<xsl:with-param name="isHeader" select="'true'"/>
								</xsl:call-template>
							</xsl:if>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="cEnumeration">
								<xsl:with-param name="isHeader" select="'true'"/>
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
				<xsl:text>&#10;</xsl:text>
			</xsl:for-each>
			<xsl:text>}&#10;</xsl:text>
			<xsl:text>&#10;</xsl:text>
			<xsl:text>#endif</xsl:text>
		</xsl:result-document>
		<!-- ################################################ Structs ############################################# -->
		<xsl:for-each select="meta/namespace/struct">
			<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
				<xsl:variable name="filename" select="concat($path,'/src/java/org/ogre4j/',@name,'.java')"/>
				<xsl:result-document href="{$filename}" format="textOutput">
					<xsl:text>package org.ogre4j;&#10;&#10;</xsl:text>
					<xsl:call-template name="struct">
						<xsl:with-param name="isGlobal" select="'true'"/>
					</xsl:call-template>
					<!-- struct methods 
				<xsl:for-each select="function">
					<xsl:call-template name="function"/>
				</xsl:for-each>-->
					<!-- structs natives 
				<xsl:for-each select="param">
					<xsl:text>	private native </xsl:text>
					<xsl:value-of select="type"/>
					<xsl:text> get</xsl:text>
					<xsl:value-of select="name"/>
					<xsl:text>(long pInstance);&#10;</xsl:text>
					<xsl:text>	private native </xsl:text>
					<xsl:value-of select="type"/>
					<xsl:text> set</xsl:text>
					<xsl:value-of select="name"/>
					<xsl:text>(long pInstance, </xsl:text>
					<xsl:value-of select="type"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="name"/>
					<xsl:text>);&#10;</xsl:text>
				</xsl:for-each> -->
				</xsl:result-document>
			</xsl:if>
		</xsl:for-each>
		<!-- ##################################### Java Proxy & Natives ######################################### -->
		<xsl:for-each select="meta/namespace/class">
			<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
				<xsl:variable name="filename" select="concat($path,'/src/java/org/ogre4j/',@name,'.java')"/>
				<xsl:result-document href="{$filename}" format="textOutput">
					<xsl:call-template name="jHeader">
						<xsl:with-param name="name" select="@name"/>
					</xsl:call-template>
					<xsl:text>package org.ogre4j;&#10;</xsl:text>
					<xsl:text>&#10;</xsl:text>
					<xsl:call-template name="imports"/>
					<xsl:call-template name="javaClasses"/>
					<xsl:text>}</xsl:text>
				</xsl:result-document>
				<!-- ################################################ C++ Header ############################################# -->
				<!-- cursor on meta/namespace/class -->
				<xsl:call-template name="cHeaderClasses"/>
				<!-- ######################################## C++ Wrapper ###################################### -->
				<!-- cursor on meta/namespace/class -->
				<xsl:call-template name="cWrapperClasses"/>
			</xsl:if>
		</xsl:for-each>
		<!-- ###################################################### ###################################################### ###################################################### -->
	</xsl:template>
	<!-- ##################################### JAVACLASSES, classes and inner classes ######################################### -->
	<xsl:template name="javaClasses">
		<xsl:variable name="currentClass" select="@name"/>
		<xsl:text>public </xsl:text>
		<xsl:choose>
			<xsl:when test="@template='true'">
				<xsl:text>interface </xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>class </xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:value-of select="@name"/>
		<xsl:if test="@template='true'">
			<xsl:text>&lt;</xsl:text>
			<xsl:value-of select="@templateDeclaration"/>
			<xsl:text>&gt;</xsl:text>
		</xsl:if>
		<!-- ######### derivation: multiple baseclasses, one baseclass, no baseclass ######## -->
		<xsl:choose>
			<!-- multiple baseclasses and template treatment -->
			<xsl:when test="count(inherits/baseClass) &gt; 1">
				<xsl:text> extends </xsl:text>
				<xsl:value-of select="inherits/baseClass[1]"/>
				<xsl:text> implements </xsl:text>
				<xsl:for-each select="inherits/baseClass">
					<xsl:variable name="baseClass" select="."/>
					<!--
					<xsl:for-each select="root()//class[@template='true']">
						<xsl:if test="@name=$baseClass">
							<xsl:text>&lt;</xsl:text>
							<xsl:if test="contains($simpleTypes,@templateDeclaration)">
								<xsl:text>Native</xsl:text>
							</xsl:if>
							<xsl:value-of select="@templateDeclaration"/>
							<xsl:text>&gt;</xsl:text>
						</xsl:if>
					</xsl:for-each>
					-->
					<xsl:if test="position() &gt; 1">
						<xsl:call-template name="interface">
							<xsl:with-param name="name" select="."/>
						</xsl:call-template>
						<xsl:if test="not(contains(current(),'&lt;'))">
							<xsl:text>I</xsl:text>
						</xsl:if>
						<xsl:value-of select="."/>
						<xsl:if test="position()!=last()">
							<xsl:text>, </xsl:text>
						</xsl:if>
					</xsl:if>
				</xsl:for-each>
			</xsl:when>
			<!-- one baseclasses and template treatment -->
			<xsl:when test="inherits">
				<xsl:variable name="baseClass" select="substring-before(inherits/baseClass,'&lt;')"/>
				<xsl:choose>
					<xsl:when test="root()/meta/namespace/class[@name=$baseClass and @template='true']">
						<xsl:text> extends NativeObject</xsl:text>
						<xsl:text> implements </xsl:text>
						<xsl:value-of select="substring-before(inherits/baseClass,'&lt;')"/>
						<xsl:text>&lt; </xsl:text>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="inherits/baseClass"/>
						</xsl:call-template>
						<xsl:text> &gt;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text> extends </xsl:text>
						<xsl:value-of select="inherits/baseClass"/>
					</xsl:otherwise>
				</xsl:choose>
				<!--
				<xsl:for-each select="root()//class[@template='true']">
					<xsl:if test="@name=$baseClass">
						<xsl:text>&lt;</xsl:text>
						<xsl:if test="contains($simpleTypes,@templateDeclaration)">
							<xsl:text>Native</xsl:text>
						</xsl:if>
						<xsl:value-of select="@templateDeclaration"/>
						<xsl:text>&gt;</xsl:text>
					</xsl:if>
				</xsl:for-each>
				-->
			</xsl:when>
			<!-- no baseclasses -->
			<xsl:otherwise>
				<xsl:if test="not(@template='true')">
					<xsl:text> extends NativeObject</xsl:text>
				</xsl:if>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text> {&#10;&#10;</xsl:text>
		<!-- inner classes -->
		<xsl:for-each select="class">
			<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
				<xsl:call-template name="javaClasses"/>
				<xsl:text>&#10;}&#10;</xsl:text>
			</xsl:if>
		</xsl:for-each>
		<!-- struct private variables -->
		<xsl:for-each select="struct">
			<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
				<xsl:call-template name="struct"/>
			</xsl:if>
		</xsl:for-each>
		<!-- typedefs -->
		<xsl:for-each select="typedef">
			<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
				<xsl:call-template name="typedef"/>
			</xsl:if>
		</xsl:for-each>
		<!-- local enumerations -->
		<xsl:for-each select="enumeration">
			<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
				<xsl:choose>
					<xsl:when test="starts-with(@name,'@')">
						<xsl:if test="not(preceding-sibling::node()[starts-with(@name,'@')])">
							<xsl:variable name="enumResultTree">
								<xsl:for-each select="../enumeration[starts-with(@name,'@')]/enum">
									<xsl:element name="enum">
										<xsl:attribute name="name" select="@name"/>
									</xsl:element>
								</xsl:for-each>
							</xsl:variable>
							<xsl:call-template name="enumeration">
								<xsl:with-param name="enumList" select="$enumResultTree"/>
							</xsl:call-template>
						</xsl:if>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="enumeration"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:for-each>
		<!-- funtion declaration -->
		<!-- cursor on meta/namespace/class -->
		<!-- constructor etc. not in templates -->
		<xsl:if test="not(@template='true')">
			<xsl:text>	protected </xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>(InstancePointer pInstance) {&#10;</xsl:text>
			<xsl:text>		super(pInstance);&#10;</xsl:text>
			<xsl:text>	}&#10;</xsl:text>
			<xsl:text>	protected </xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>(InstancePointer pInstance, boolean createdByLibrary) {&#10;</xsl:text>
			<xsl:text>		super(pInstance, createdByLibrary);&#10;</xsl:text>
			<xsl:text>	}&#10;</xsl:text>
			<!-- dispose() -->
			<xsl:text>	public void dispose() {&#10;</xsl:text>
			<xsl:choose>
				<xsl:when test="$config/config/dispose_exception/@use='true'">
					<xsl:text>	if (this.createdByLibrary)&#10;</xsl:text>
					<xsl:text>		throw new RuntimeException("can't dispose object created by the library");&#10;</xsl:text>
				</xsl:when>
				<xsl:when test="$config/config/dispose_exception/@use='false'">
					<xsl:text>	if (this.createdByLibrary) return;&#10;</xsl:text>
				</xsl:when>
			</xsl:choose>
			<xsl:text>		_dispose(pInstance.value);&#10;</xsl:text>
			<xsl:text>	}&#10;</xsl:text>
		</xsl:if>
		<!-- vector, getter and setter -->
		<xsl:if test="$config/config/vecmath='false' and contains($vectors, @name)">
			<xsl:text>	public void setX(float x) {&#10;</xsl:text>
			<xsl:text>		_setX(pInstance.value, x);&#10;</xsl:text>
			<xsl:text>	}&#10;</xsl:text>
			<xsl:text>	public void setY(float y) {&#10;</xsl:text>
			<xsl:text>		_setY(pInstance.value, y);&#10;</xsl:text>
			<xsl:text>	}&#10;</xsl:text>
			<xsl:text>	public void setZ(float z) {&#10;</xsl:text>
			<xsl:text>		_setZ(pInstance.value, z);&#10;</xsl:text>
			<xsl:text>	}&#10;</xsl:text>
			<xsl:text>	public float getX() {&#10;</xsl:text>
			<xsl:text>		return _getX(pInstance.value);&#10;</xsl:text>
			<xsl:text>	}&#10;</xsl:text>
			<xsl:text>	public float getY() {&#10;</xsl:text>
			<xsl:text>		return _getY(pInstance.value);&#10;</xsl:text>
			<xsl:text>	}&#10;</xsl:text>
			<xsl:text>	public float getZ() {&#10;</xsl:text>
			<xsl:text>		return _getZ(pInstance.value);&#10;</xsl:text>
			<xsl:text>	}&#10;</xsl:text>
		</xsl:if>
		<!-- methods of the implemented interfaces -->
		<xsl:if test="count(inherits/baseClass) &gt; 1">
			<xsl:for-each select="inherits/baseClass">
				<xsl:if test="position() &gt; 1">
					<xsl:variable name="interface" select="."/>
					<xsl:for-each select="root()//class[@name=$interface]">
						<xsl:for-each select="function[@virt!='pure-virtual']">
							<xsl:if test="name!=../@name">
								<xsl:call-template name="function"/>
							</xsl:if>
						</xsl:for-each>
					</xsl:for-each>
				</xsl:if>
			</xsl:for-each>
		</xsl:if>
		<!-- methods of the implemented template -->
		<xsl:if test="inherits">
			<xsl:for-each select="inherits/baseClass">
				<xsl:if test="contains(current(),'&lt;')">
					<xsl:variable name="baseClass" select="substring-before(current(),'&lt;')"/>
					<xsl:for-each select="root()//class[@name=$baseClass]">
						<!--<xsl:if test="@template='true'">-->
						<xsl:for-each select="function">
							<xsl:if test="name!=../@name">
								<xsl:call-template name="function">
									<xsl:with-param name="isTemplate" select="'true'"/>
									<xsl:with-param name="currentClass" select="$currentClass"/>
								</xsl:call-template>
							</xsl:if>
						</xsl:for-each>
						<!--</xsl:if>-->
					</xsl:for-each>
				</xsl:if>
			</xsl:for-each>
		</xsl:if>
		<!-- in class functions -->
		<xsl:for-each select="function">
			<!-- not the methods of the implemented template -->
			<xsl:variable name="baseClass">
				<xsl:for-each select="../inherits/baseClass">
					<xsl:element name="name">
						<xsl:value-of select="substring-before(.,'&lt;')"/>
					</xsl:element>
				</xsl:for-each>
			</xsl:variable>
			<xsl:if test="not(name=root()//class[@name=$baseClass/name]/function/name)">
				<xsl:call-template name="function"/>
			</xsl:if>
		</xsl:for-each>
		<!-- ##################################### Java Natives ##################################### -->
		<!-- cursor on meta/namespace/class -->
		<!-- vector, getter und setter -->
		<xsl:if test="not(@template='true')">
			<xsl:if test="$config/config/vecmath='false' and contains($vectors, @name)">
				<xsl:text>	private native void _setX(long pInstance, float x);&#10;</xsl:text>
				<xsl:text>	private native void _setY(long pInstance, float y);&#10;</xsl:text>
				<xsl:text>	private native void _setZ(long pInstance, float z);&#10;</xsl:text>
				<xsl:text>	private native float _getX(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native float _getY(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native float _getZ(long pInstance);&#10;</xsl:text>
			</xsl:if>
			<!-- methods of the implemented interfaces -->
			<!--<xsl:choose>
				<xsl:when test="count(inherits/baseClass) &gt; 1">
					<xsl:for-each select="inherits/baseClass">
						<xsl:if test="position() &gt; 1">
							<xsl:variable name="interface" select="."/>
							<xsl:for-each select="root()//class[@name=$interface]">
								<xsl:for-each select="function[@virt!='pure-virtual']">
									<xsl:if test="name!=../@name">
										<xsl:call-template name="nativeFunction">
											<xsl:with-param name="isTemplate" select="'true'"/>
											<xsl:with-param name="currentClass" select="$currentClass"/>
										</xsl:call-template>
									</xsl:if>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:if>
					</xsl:for-each>
				</xsl:when>-->
			<!-- methods of the implemented interfaces -->
			<xsl:if test="count(inherits/baseClass) &gt; 1">
				<xsl:for-each select="inherits/baseClass">
					<xsl:if test="position() &gt; 1">
						<xsl:variable name="interface" select="."/>
						<xsl:for-each select="root()//class[@name=$interface]">
							<xsl:for-each select="function[@virt!='pure-virtual']">
								<xsl:if test="name!=../@name">
									<xsl:call-template name="nativeFunction">
										<xsl:with-param name="isTemplate" select="'true'"/>
										<xsl:with-param name="currentClass" select="$currentClass"/>
									</xsl:call-template>
								</xsl:if>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:if>
				</xsl:for-each>
			</xsl:if>
			<!-- methods of the implemented template -->
			<xsl:if test="inherits">
				<xsl:for-each select="inherits/baseClass">
					<xsl:if test="contains(current(),'&lt;')">
						<xsl:variable name="baseClass" select="substring-before(current(),'&lt;')"/>
						<xsl:for-each select="root()//class[@name=$baseClass]">
							<!--<xsl:if test="@template='true'">-->
							<xsl:for-each select="function">
								<xsl:if test="name!=../@name">
									<xsl:call-template name="nativeFunction">
										<xsl:with-param name="isTemplate" select="'true'"/>
										<xsl:with-param name="currentClass" select="$currentClass"/>
									</xsl:call-template>
								</xsl:if>
							</xsl:for-each>
							<!--</xsl:if>-->
						</xsl:for-each>
					</xsl:if>
				</xsl:for-each>
			</xsl:if>
			<!-- methods of the implemented template 
				<xsl:when test="inherits">
					<xsl:variable name="baseClass" select="substring-before(inherits/baseClass,'&lt;')"/>
					<xsl:for-each select="root()//class[@name=$baseClass]">
						<xsl:if test="@template='true'">
							<xsl:for-each select="function">
								<xsl:if test="name!=../@name">
									<xsl:call-template name="nativeFunction">
										<xsl:with-param name="isTemplate" select="'true'"/>
										<xsl:with-param name="currentClass" select="$currentClass"/>
									</xsl:call-template>
								</xsl:if>
							</xsl:for-each>
						</xsl:if>
					</xsl:for-each>
				</xsl:when>
			</xsl:choose>-->
			<!-- own methods -->
			<xsl:for-each select="function">
				<!-- no methods of the implemented template -->
				<xsl:variable name="baseClass">
					<xsl:for-each select="../inherits/baseClass">
						<xsl:element name="name">
							<xsl:value-of select="substring-before(.,'&lt;')"/>
						</xsl:element>
					</xsl:for-each>
				</xsl:variable>
				<xsl:if test="not(name=root()//class[@name=$baseClass/name]/function/name)">
					<xsl:call-template name="nativeFunction">
						<xsl:with-param name="isTemplate" select="'true'"/>
						<xsl:with-param name="currentClass" select="$currentClass"/>
					</xsl:call-template>
				</xsl:if>
			</xsl:for-each>
			<xsl:text>	private native void _dispose(long pInstance);&#10;</xsl:text>
		</xsl:if>
	</xsl:template>
	<!-- ################################################ CHEADERCLASSES ############################################# -->
	<xsl:template name="cHeaderClasses">
		<!-- recursion for inner classes -->
		<xsl:for-each select="class">
			<xsl:call-template name="cHeaderClasses"/>
		</xsl:for-each>
		<xsl:variable name="filename" select="concat($path,'/src/native/include/jni/org_ogre4j_',@name,'.h')"/>
		<xsl:result-document href="{$filename}" format="textOutput">
			<xsl:text>/* DO NOT EDIT THIS FILE - it is machine generated */&#10;</xsl:text>
			<xsl:text>#include &lt;jni.h&gt;&#10;</xsl:text>
			<xsl:text>/* Header for class org_ogre4j_</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>*/&#10;</xsl:text>
			<xsl:text>&#10;</xsl:text>
			<xsl:text>#ifndef _Included_org_ogre4j_</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>_&#10;</xsl:text>
			<xsl:text>#define _Included_org_ogre4j_</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>_</xsl:text>
			<xsl:text>&#10;</xsl:text>
			<xsl:text>#ifdef __cplusplus&#10;</xsl:text>
			<xsl:text>extern "C" {&#10;</xsl:text>
			<xsl:text>#endif&#10;</xsl:text>
			<xsl:text>&#10;</xsl:text>
			<!-- vector, getter and setter -->
			<xsl:if test="$config/config/vecmath='false' and contains($vectors, @name)">
				<xsl:text>JNIEXPORT void JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setX(JNIEnv *, jobject, jlong, jfloat);&#10;</xsl:text>
				<xsl:text>JNIEXPORT void JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setY(JNIEnv *, jobject, jlong, jfloat);&#10;</xsl:text>
				<xsl:text>JNIEXPORT void JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setZ(JNIEnv *, jobject, jlong, jfloat);&#10;</xsl:text>
				<xsl:text>JNIEXPORT jfloat JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setX(JNIEnv *, jobject, jlong);&#10;</xsl:text>
				<xsl:text>JNIEXPORT jfloat JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setY(JNIEnv *, jobject, jlong);&#10;</xsl:text>
				<xsl:text>JNIEXPORT jfloat JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setZ(JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:if>
			<!-- structs
				<xsl:for-each select="struct">
					<xsl:for-each select="param">
						<xsl:call-template name="comment"/>
						<xsl:text>JNIEXPORT j</xsl:text>
						<xsl:value-of select="type"/>
						<xsl:text> JNICALL Java_org_ogre4j_ get</xsl:text>
						<xsl:value-of select="name"/>
						<xsl:text>(JNIEnv *, jclass, jlong);&#10;</xsl:text>
						<xsl:text>JNIEXPORT j</xsl:text>
						<xsl:value-of select="type"/>
						<xsl:text> JNICALL Java_org_ogre4j_ set</xsl:text>
						<xsl:value-of select="name"/>
						<xsl:text>(JNIEnv *, jclass, jlong, j</xsl:text>
						<xsl:value-of select="type"/>
						<xsl:text>);&#10;</xsl:text>
					</xsl:for-each>
				</xsl:for-each> -->
			<xsl:for-each select="function">
				<xsl:if test="not(contains(name,'operator')) and not(starts-with(type,'OGRE'))">
					<xsl:choose>
						<!-- constructor -->
						<!--							<xsl:when test="not(type)"/>-->
						<!-- functions -->
						<!--							<xsl:when test="type">-->
						<xsl:when test="1=1">
							<xsl:call-template name="comment"/>
							<xsl:call-template name="cDeclaration"/>
							<xsl:choose>
								<xsl:when test="name=../@name">
									<xsl:text>(JNIEnv *, jobject</xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
											<xsl:text>(JNIEnv *, jclass, jlong</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text>(JNIEnv *, jobject, jlong</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:for-each select="parameters/parameter">
								<xsl:if test="type!='void'">
									<xsl:choose>
										<xsl:when test="type and not(contains($simpleTypes,type))">
											<xsl:text>, j</xsl:text>
											<xsl:text>long</xsl:text>
										</xsl:when>
										<xsl:when test="type='String'">
											<xsl:text>, jstring</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text>, j</xsl:text>
											<xsl:value-of select="type"/>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:if>
							</xsl:for-each>
							<xsl:text>)</xsl:text>
							<xsl:text>;&#10;</xsl:text>
						</xsl:when>
					</xsl:choose>
				</xsl:if>
			</xsl:for-each>
			<!-- dispose() -->
			<xsl:text>&#10;</xsl:text>
			<xsl:text>/*&#10;</xsl:text>
			<xsl:text> * Class:     org_ogre4j_</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>&#10;</xsl:text>
			<xsl:text> * Method:    dispose&#10;</xsl:text>
			<xsl:text> * Signature: (I)V&#10;</xsl:text>
			<xsl:text> */&#10;</xsl:text>
			<xsl:text>JNIEXPORT void JNICALL Java_org_ogre4j_</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>_dispose (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			<xsl:text>&#10;</xsl:text>
			<xsl:text>#ifdef __cplusplus&#10;</xsl:text>
			<xsl:text>}&#10;</xsl:text>
			<xsl:text>#endif&#10;</xsl:text>
			<xsl:text>#endif</xsl:text>
		</xsl:result-document>
	</xsl:template>
	<!-- ######################################## CWRAPPERCLASSES ###################################### -->
	<!-- cursor on meta/namespace/class -->
	<xsl:template name="cWrapperClasses">
		<!-- recursion for inner classes -->
		<xsl:for-each select="class">
			<xsl:call-template name="cWrapperClasses"/>
		</xsl:for-each>
		<xsl:variable name="prefix" select="$config/config/prefix_cpp"/>
		<xsl:variable name="filename" select="concat($path,'/src/native/src/',$prefix,'_',@name,'.cpp')"/>
		<xsl:result-document href="{$filename}" format="textOutput">
			<xsl:call-template name="cHeader">
				<xsl:with-param name="fullName" select="@fullName"/>
			</xsl:call-template>
			<xsl:variable name="className" select="@name"/>
			<!-- all headers included in Precompiled.h (almost all)
					<xsl:choose>
						<xsl:when test="$config/config/precompiled_headers/@use ='true'">
							<xsl:text>#include &lt;Precompiled.h&gt;&#10;</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text>#include &lt;</xsl:text>
							<xsl:value-of select="concat('Ogre',$className,'.h')"/>
							<xsl:text>&gt;&#10;</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
					-->
			<xsl:text>#include &lt;Precompiled.h&gt;&#10;</xsl:text>
			<!--
					<xsl:text>#include &lt;</xsl:text>
					<xsl:value-of select="concat('Ogre',$className,'.h')"/>
					<xsl:text>&gt;&#10;</xsl:text>
					-->
			<xsl:text>#include &lt;</xsl:text>
			<xsl:value-of select="concat('org_ogre4j_',@name,'.h')"/>
			<xsl:text>&gt;&#10;</xsl:text>
			<xsl:text>&#10;</xsl:text>
			<xsl:text>&#10;</xsl:text>
			<!-- start -->
			<xsl:variable name="namespace" select="../@name"/>
			<xsl:variable name="class" select="@name"/>
			<!-- vector, getter und setter -->
			<xsl:if test="$config/config/vecmath='false' and contains($vectors, @name)">
				<xsl:text>JNIEXPORT void JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setX(JNIEnv *env, jobject that, jlong pInstance, jfloat x) {&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>* p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text> = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:text>p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>->setX(x);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
				<xsl:text>JNIEXPORT void JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setY(JNIEnv *env, jobject that, jlong pInstance, jfloat y) {&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>* p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text> = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:text>p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>->setY(y);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
				<xsl:text>JNIEXPORT void JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setZ(JNIEnv *env, jobject that, jlong pInstance, jfloat z) {&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>* p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text> = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:text>p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>->setZ(z);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
				<xsl:text>JNIEXPORT jfloat JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setX(JNIEnv *env, jobject that, jlong pInstance) {&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>* p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text> = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:text>return p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>->getX();&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
				<xsl:text>JNIEXPORT jfloat JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setY(JNIEnv *env, jobject that, jlong pInstance) {&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>* p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text> = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:text>return p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>->getY();&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
				<xsl:text>JNIEXPORT jfloat JNICALL Java_org_ogre4j_</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>_setZ(JNIEnv *env, jobject that, jlong pInstance) {&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>* p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text> = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="../@name"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:text>return p</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>->getZ();&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:if>
			<!-- structs 
				<xsl:for-each select="struct">
					<xsl:for-each select="param">
						<xsl:text>JNIEXPORT j</xsl:text>
						<xsl:value-of select="type"/>
						<xsl:text> JNICALL Java_org_ogre4j_ get</xsl:text>
						<xsl:value-of select="name"/>
						<xsl:text>(JNIEnv *env, jclass that, jlong pInstance) {&#10;</xsl:text>
						<xsl:text>	</xsl:text>
						<xsl:value-of select="../../../@name"/>
						<xsl:text>::</xsl:text>
						<xsl:value-of select="../../@name"/>
						<xsl:text>* p</xsl:text>
						<xsl:value-of select="../../@name"/>
						<xsl:text> = reinterpret_cast&lt;</xsl:text>
						<xsl:value-of select="../../../@name"/>
						<xsl:text>::</xsl:text>
						<xsl:value-of select="../../@name"/>
						<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
						<xsl:text>	return p</xsl:text>
						<xsl:value-of select="../@name"/>
						<xsl:text>-></xsl:text>
						<xsl:value-of select="name"/>
						<xsl:text>;&#10;</xsl:text>
						<xsl:text>}&#10;</xsl:text>
						
						<xsl:text>JNIEXPORT j</xsl:text>
						<xsl:value-of select="type"/>
						<xsl:text> JNICALL Java_org_ogre4j_ set</xsl:text>
						<xsl:value-of select="name"/>
						<xsl:text>(JNIEnv *env, jclass that, jlong pInstance, j</xsl:text>
						<xsl:value-of select="type"/>
						<xsl:text> </xsl:text>
						<xsl:value-of select="name"/>
						<xsl:text>) {&#10;</xsl:text>
						<xsl:text>	</xsl:text>
						<xsl:value-of select="../../../@name"/>
						<xsl:text>::</xsl:text>
						<xsl:value-of select="../../@name"/>
						<xsl:text>* p</xsl:text>
						<xsl:value-of select="../../@name"/>
						<xsl:text> = reinterpret_cast&lt;</xsl:text>
						<xsl:value-of select="../../../@name"/>
						<xsl:text>::</xsl:text>
						<xsl:value-of select="../../@name"/>
						<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
						<xsl:text>	p</xsl:text>
						<xsl:value-of select="../@name"/>
						<xsl:text>-></xsl:text>
						<xsl:value-of select="name"/>
						<xsl:text> = </xsl:text>
						<xsl:value-of select="name"/>
						<xsl:text>;&#10;</xsl:text>
						<xsl:text>	return p</xsl:text>
						<xsl:value-of select="../@name"/>
						<xsl:text>-></xsl:text>
						<xsl:value-of select="name"/>
						<xsl:text>;&#10;</xsl:text>
						<xsl:text>}&#10;</xsl:text>
					</xsl:for-each>
				</xsl:for-each>-->
			<!-- typedefs -->
			<xsl:for-each select="typedef">
				<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
					<xsl:call-template name="cTypedef"/>
				</xsl:if>
			</xsl:for-each>
			<!-- in class enumerations -->
			<xsl:for-each select="enumeration">
				<xsl:if test="not(@name=$ignore/ignorelist/ignore)">
					<xsl:choose>
						<xsl:when test="starts-with(@name,'@')">
							<xsl:if test="not(preceding-sibling::node()[starts-with(@name,'@')])">
								<xsl:variable name="enumResultTree">
									<xsl:for-each select="../enumeration[starts-with(@name,'@')]/enum">
										<xsl:element name="enum">
											<xsl:attribute name="name" select="@name"/>
										</xsl:element>
									</xsl:for-each>
								</xsl:variable>
								<xsl:call-template name="cEnumeration">
									<xsl:with-param name="enumList" select="$enumResultTree"/>
								</xsl:call-template>
							</xsl:if>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="cEnumeration"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</xsl:for-each>
			<!-- in class functions -->
			<xsl:for-each select="function">
				<xsl:if test="name!=../@name and not(contains(name,'operator')) and not(starts-with(type,'OGRE')) and type">
					<!-- ##### function declaration ##### -->
					<xsl:call-template name="comment"/>
					<xsl:call-template name="cDeclaration"/>
					<xsl:choose>
						<xsl:when test="name=../@name">
							<xsl:text>(JNIEnv *env, jobject that</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:choose>
								<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
									<xsl:text>(JNIEnv *env, jobject that, jlong pInstance</xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<xsl:text>(JNIEnv *env, jobject that, jlong pInstance</xsl:text>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:for-each select="parameters/parameter">
						<xsl:if test="type!='void'">
							<xsl:choose>
								<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
									<xsl:text>, jint</xsl:text>
								</xsl:when>
								<xsl:when test="type and not(contains($simpleTypes,type))">
									<xsl:text>, jlong</xsl:text>
								</xsl:when>
								<xsl:when test="type='String'">
									<xsl:text>, jstring</xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<xsl:text>, j</xsl:text>
									<xsl:value-of select="type"/>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:text> </xsl:text>
							<xsl:value-of select="name"/>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>)</xsl:text>
					<xsl:text> {&#10;</xsl:text>
					<xsl:choose>
						<xsl:when test="$config/config/exception_handling/@use='0'"/>
						<xsl:when test="$config/config/exception_handling/@use='1'">
							<xsl:text>	try {&#10;</xsl:text>
						</xsl:when>
						<xsl:when test="$config/config/exception_handling/@use='2'"/>
						<xsl:otherwise>unhandeled usage option in	file: cpp2java.cfg.xslt	node: config/exception_handling/@use</xsl:otherwise>
					</xsl:choose>
					<!-- ##### function implementation ##### -->
					<!-- first line always the same -->
					<xsl:text>	</xsl:text>
					<xsl:value-of select="../../@name"/>
					<xsl:text>::</xsl:text>
					<xsl:value-of select="../@name"/>
					<xsl:text> * p</xsl:text>
					<xsl:value-of select="../@name"/>
					<xsl:text> = reinterpret_cast&lt;</xsl:text>
					<xsl:value-of select="../../@name"/>
					<xsl:text>::</xsl:text>
					<xsl:value-of select="../@name"/>
					<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
					<xsl:choose>
						<!-- no constructor -->
						<xsl:when test="name=../@name"/>
						<!-- ######## parameter type is a struct ######## -->
						<xsl:when test="parameters/parameter/type=root()//struct/@name">
							<xsl:variable name="structType" select="parameters/parameter/type"/>
							<xsl:variable name="structName" select="parameters/parameter/name"/>
							<xsl:text>	</xsl:text>
							<xsl:value-of select="../../@name"/>
							<xsl:text>::</xsl:text>
							<xsl:value-of select="$structType"/>
							<xsl:text> tmp</xsl:text>
							<xsl:value-of select="$structType"/>
							<xsl:text>;&#10;</xsl:text>
							<xsl:text>	jclass cls = env->GetObjectClass(</xsl:text>
							<xsl:value-of select="$structName"/>
							<xsl:text>);&#10;</xsl:text>
							<xsl:for-each select="root()//struct">
								<xsl:if test="@name=$structType">
									<xsl:text>&#10;</xsl:text>
									<xsl:for-each select="param">
										<xsl:text>	jfieldID fid_</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:text>;&#10;</xsl:text>
										<xsl:text>	fid_</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:text> = env-&gt;GetFieldID(cls, "</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:text>", "F");&#10;</xsl:text>
										<xsl:text>	tmp</xsl:text>
										<xsl:value-of select="$structType"/>
										<xsl:text>.</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:text> = env-&gt;GetFloatField(obj, fid_</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:text>);&#10;</xsl:text>
										<xsl:text>	env-&gt;SetFloatField(</xsl:text>
										<xsl:value-of select="$structName"/>
										<xsl:text>, fid_</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:text>	tmp</xsl:text>
										<xsl:value-of select="$structType"/>
										<xsl:text>.</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:text>);&#10;</xsl:text>
									</xsl:for-each>
								</xsl:if>
								<xsl:text>&#10;</xsl:text>
							</xsl:for-each>
							<xsl:text>	p</xsl:text>
							<xsl:value-of select="../@name"/>
							<xsl:text>-&gt;</xsl:text>
							<xsl:value-of select="name"/>
							<xsl:text>(&amp;tmp</xsl:text>
							<xsl:value-of select="$structType"/>
							<xsl:text>);&#10;</xsl:text>
						</xsl:when>
						<!-- ######## return value void ######### -->
						<xsl:when test="type='void'">
							<!-- calling the toEnum function for enum parameters -->
							<xsl:for-each select="parameters/parameter">
								<xsl:if test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
									<xsl:text>	</xsl:text>
									<xsl:value-of select="$namespace"/>
									<xsl:text>::</xsl:text>
									<xsl:choose>
										<xsl:when test="contains(type,'.')">
											<xsl:value-of select="substring-before(type,'.')"/>
											<xsl:text>::</xsl:text>
											<xsl:value-of select="substring-after(type,'.')"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="type"/>
										</xsl:otherwise>
									</xsl:choose>
									<xsl:text> </xsl:text>
									<xsl:value-of select="name"/>
									<xsl:text>Enum = Ogre4J::to</xsl:text>
									<xsl:choose>
										<xsl:when test="contains(type,'.')">
											<xsl:value-of select="substring-after(type,'.')"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="type"/>
										</xsl:otherwise>
									</xsl:choose>
									<xsl:text>Enum(</xsl:text>
									<xsl:value-of select="name"/>
									<xsl:text>);&#10;</xsl:text>
								</xsl:if>
							</xsl:for-each>
							<xsl:choose>
								<xsl:when test="parameters/parameter/type='String'">
									<xsl:for-each select="parameters/parameter">
										<xsl:if test="type='String'">
											<xsl:text>	</xsl:text>
											<xsl:value-of select="../../../../@name"/>
											<xsl:text>::</xsl:text>
											<xsl:value-of select="type"/>
											<xsl:text> s</xsl:text>
											<xsl:value-of select="name"/>
											<xsl:text>;&#10;</xsl:text>
											<xsl:text>	</xsl:text>
											<xsl:text>Ogre4JHelper::CopyJString( env, </xsl:text>
											<xsl:value-of select="name"/>
											<xsl:text>, s</xsl:text>
											<xsl:value-of select="name"/>
											<xsl:text>);&#10;</xsl:text>
											<xsl:text>	</xsl:text>
											<xsl:text>p</xsl:text>
											<xsl:value-of select="../../../@name"/>
											<xsl:text>-></xsl:text>
											<xsl:value-of select="../../name"/>
											<xsl:text>(s</xsl:text>
											<xsl:value-of select="name"/>
											<xsl:text>);&#10;</xsl:text>
										</xsl:if>
									</xsl:for-each>
								</xsl:when>
								<xsl:otherwise>
									<xsl:text>	</xsl:text>
									<xsl:text>p</xsl:text>
									<xsl:value-of select="../@name"/>
									<xsl:text>-></xsl:text>
									<xsl:value-of select="name"/>
									<xsl:text>(</xsl:text>
									<xsl:for-each select="parameters/parameter">
										<xsl:if test="type!='void'">
											<xsl:choose>
												<!-- parameter is a primitive type -->
												<xsl:when test="contains($simpleTypes,type)">
													<xsl:value-of select="name"/>
													<xsl:if test="type='boolean'">
														<xsl:text>?true:false</xsl:text>
													</xsl:if>
													<xsl:if test="position()!=last()">
														<xsl:text>, </xsl:text>
													</xsl:if>
												</xsl:when>
												<!-- parameter is an enum -->
												<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
													<xsl:value-of select="name"/>
													<xsl:text>Enum</xsl:text>
												</xsl:when>
												<!-- parameter passed by pointer -->
												<xsl:when test="@passedBy='pointer'">
													<xsl:text>reinterpret_cast&lt;</xsl:text>
													<xsl:value-of select="../../../../@name"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text>*&gt;(</xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>)</xsl:text>
													<xsl:if test="position()!=last()">
														<xsl:text>, </xsl:text>
													</xsl:if>
												</xsl:when>
												<!-- parameter passed by reference -->
												<xsl:when test="@passedBy='reference'">
													<xsl:text>reinterpret_cast&lt;</xsl:text>
													<xsl:value-of select="../../../../@name"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text>&amp;&gt;(</xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>)</xsl:text>
													<xsl:if test="position()!=last()">
														<xsl:text>, </xsl:text>
													</xsl:if>
												</xsl:when>
												<!-- parameter passed by value -->
												<xsl:when test="@passedBy='value'">
													<xsl:text>reinterpret_cast&lt;</xsl:text>
													<xsl:value-of select="../../../../@name"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text>&gt;(</xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>)</xsl:text>
													<xsl:if test="position()!=last()">
														<xsl:text>, </xsl:text>
													</xsl:if>
												</xsl:when>
											</xsl:choose>
										</xsl:if>
									</xsl:for-each>
									<xsl:text>);&#10;</xsl:text>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<!-- ###### any return value, with parameters ###### -->
						<xsl:when test="type and parameters">
							<xsl:choose>
								<!-- funcion type String -->
								<xsl:when test="type='String'">
									<xsl:text>	return env->NewStringUTF(p</xsl:text>
									<xsl:value-of select="../@name"/>
									<xsl:text>-></xsl:text>
									<xsl:value-of select="name"/>
									<xsl:text>().c_str());&#10;</xsl:text>
								</xsl:when>
								<!-- function type Vector3 -->
								<xsl:when test="type='Vector3'">
									<xsl:text>	</xsl:text>
									<xsl:value-of select="../../@name"/>
									<xsl:text>::</xsl:text>
									<xsl:value-of select="type"/>
									<xsl:if test="@passedBy='pointer'">
										<xsl:text> *</xsl:text>
									</xsl:if>
									<xsl:text> p</xsl:text>
									<xsl:value-of select="type"/>
									<xsl:text> = p</xsl:text>
									<xsl:value-of select="../@name"/>
									<xsl:text>-></xsl:text>
									<xsl:value-of select="name"/>
									<xsl:text>(</xsl:text>
									<xsl:for-each select="parameters/parameter">
										<!-- parameter type simpleTypes -->
										<xsl:choose>
											<xsl:when test="contains($simpleTypes,type)">
												<xsl:value-of select="name"/>
												<xsl:if test="position()!=last()">
													<xsl:text>, </xsl:text>
												</xsl:if>
											</xsl:when>
											<xsl:otherwise>
												<xsl:if test="type!='void'">
													<xsl:text>reinterpret_cast&lt;cl*&gt;(</xsl:text>
													<xsl:value-of select="../../../../@name"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text>, </xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>)</xsl:text>
												</xsl:if>
												<xsl:if test="position()!=last()">
													<xsl:text>, </xsl:text>
												</xsl:if>
											</xsl:otherwise>
										</xsl:choose>
									</xsl:for-each>
									<xsl:text>);&#10;</xsl:text>
									<xsl:text>	jfloat values[3] = { pVector3.x, pVector3.y, pVector3.z};&#10;</xsl:text>
									<xsl:text>	jfloatArray vector3 = env->NewFloatArray( 3 );&#10;</xsl:text>
									<xsl:text>	env->SetFloatArrayRegion( vector3, 0, 3, values);&#10;</xsl:text>
									<xsl:text>	return reinterpret_cast&lt;jlong&gt;(vector3);&#10;</xsl:text>
								</xsl:when>
								<!-- parameter type void -->
								<xsl:when test="type and parameters/parameter/type='void'">
									<xsl:choose>
										<!-- shared pointer -->
										<xsl:when test="@passedBy='reference'">
											<xsl:text>	</xsl:text>
											<xsl:value-of select="../../@name"/>
											<xsl:text>::</xsl:text>
											<xsl:value-of select="type"/>
											<xsl:text> p</xsl:text>
											<xsl:value-of select="type"/>
											<xsl:text> = p</xsl:text>
											<xsl:value-of select="../@name"/>
											<xsl:text>-></xsl:text>
											<xsl:value-of select="name"/>
											<xsl:text>();&#10;</xsl:text>
											<xsl:text>	</xsl:text>
											<xsl:value-of select="../../@name"/>
											<xsl:text>::</xsl:text>
											<xsl:value-of select="type"/>
											<xsl:text> * ptr = new </xsl:text>
											<xsl:value-of select="../../@name"/>
											<xsl:text>::</xsl:text>
											<xsl:value-of select="type"/>
											<xsl:text>(p</xsl:text>
											<xsl:value-of select="type"/>
											<xsl:text>);&#10;</xsl:text>
											<xsl:choose>
												<xsl:when test="not(contains(type,$simpleTypes))">
													<xsl:text>	return reinterpret_cast&lt;jlong&gt;(ptr);&#10;</xsl:text>
												</xsl:when>
												<xsl:otherwise>
													<xsl:text>	return ptr;&#10;</xsl:text>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:when>
										<!-- enum -->
										<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
											<xsl:text>	return toInt(p</xsl:text>
											<xsl:value-of select="../@name"/>
											<xsl:text>-></xsl:text>
											<xsl:value-of select="name"/>
											<xsl:text>());&#10;</xsl:text>
										</xsl:when>
										<!-- return by pointer -->
										<xsl:when test="@passedBy='pointer'">
											<xsl:text>	return reinterpret_cast&lt;jlong&gt;(p</xsl:text>
											<xsl:value-of select="../@name"/>
											<xsl:text>-></xsl:text>
											<xsl:value-of select="name"/>
											<xsl:text>());&#10;</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text>	return p</xsl:text>
											<xsl:value-of select="../@name"/>
											<xsl:text>-></xsl:text>
											<xsl:value-of select="name"/>
											<xsl:text>();&#10;</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<!-- function type simpleTypes -->
								<xsl:when test="contains($simpleTypes,type)">
									<xsl:text>	return p</xsl:text>
									<xsl:value-of select="../@name"/>
									<xsl:text>-></xsl:text>
									<xsl:value-of select="name"/>
									<xsl:text>(</xsl:text>
									<xsl:for-each select="parameters/parameter">
										<xsl:value-of select="name"/>
										<xsl:if test="type='boolean'">
											<xsl:text>?true:false</xsl:text>
										</xsl:if>
										<xsl:if test="position()!=last()">
											<xsl:text>, </xsl:text>
										</xsl:if>
									</xsl:for-each>
									<xsl:text>);&#10;</xsl:text>
								</xsl:when>
								<!-- funktion type not(simpleTypes) -->
								<xsl:when test="not(contains($simpleTypes,type))">
									<xsl:choose>
										<!-- function type not(simpleTypes) (by pointer) -->
										<xsl:when test="@passedBy='pointer'">
											<!-- calling the toEnum function for enum parameters -->
											<xsl:for-each select="parameters/parameter">
												<xsl:if test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
													<xsl:text>	</xsl:text>
													<xsl:value-of select="$namespace"/>
													<xsl:text>::</xsl:text>
													<xsl:choose>
														<xsl:when test="contains(type,'.')">
															<xsl:value-of select="substring-before(type,'.')"/>
															<xsl:text>::</xsl:text>
															<xsl:value-of select="substring-after(type,'.')"/>
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="type"/>
														</xsl:otherwise>
													</xsl:choose>
													<xsl:text> </xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>Enum = Ogre4J::to</xsl:text>
													<xsl:choose>
														<xsl:when test="contains(type,'.')">
															<xsl:value-of select="substring-after(type,'.')"/>
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="type"/>
														</xsl:otherwise>
													</xsl:choose>
													<xsl:text>Enum(</xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>);&#10;</xsl:text>
												</xsl:if>
												<!-- parameter is a string -->
												<xsl:if test="type='String'">
													<xsl:text>	</xsl:text>
													<xsl:value-of select="../../../../@name"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text> s</xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>;&#10;</xsl:text>
													<xsl:text>	</xsl:text>
													<xsl:text>Ogre4JHelper::CopyJString( env, </xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>, s</xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>);&#10;</xsl:text>
												</xsl:if>
											</xsl:for-each>
											<xsl:text>	</xsl:text>
											<xsl:value-of select="../../@name"/>
											<xsl:text>::</xsl:text>
											<xsl:value-of select="type"/>
											<xsl:text> * ptr</xsl:text>
											<xsl:value-of select="type"/>
											<xsl:text> = p</xsl:text>
											<xsl:value-of select="../@name"/>
											<xsl:text>-></xsl:text>
											<xsl:value-of select="name"/>
											<xsl:text>(</xsl:text>
											<xsl:for-each select="parameters/parameter">
												<xsl:choose>
													<!-- parameter is a primitive type -->
													<xsl:when test="contains($simpleTypes,type)">
														<xsl:value-of select="name"/>
														<xsl:if test="type='boolean'">
															<xsl:text>?true:false</xsl:text>
														</xsl:if>
														<xsl:if test="position()!=last()">
															<xsl:text>, </xsl:text>
														</xsl:if>
													</xsl:when>
													<!-- parameter is an enum -->
													<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
														<xsl:value-of select="name"/>
														<xsl:text>Enum</xsl:text>
													</xsl:when>
													<!-- parameter passed by pointer -->
													<xsl:when test="@passedBy='pointer'">
														<xsl:text>reinterpret_cast&lt;</xsl:text>
														<xsl:value-of select="../../../../@name"/>
														<xsl:text>::</xsl:text>
														<xsl:value-of select="type"/>
														<xsl:text>*&gt;(</xsl:text>
														<xsl:value-of select="name"/>
														<xsl:text>)</xsl:text>
														<xsl:if test="position()!=last()">
															<xsl:text>, </xsl:text>
														</xsl:if>
													</xsl:when>
													<!-- parameter passed by reference -->
													<xsl:when test="@passedBy='reference'">
														<xsl:text>reinterpret_cast&lt;</xsl:text>
														<xsl:value-of select="../../../../@name"/>
														<xsl:text>::</xsl:text>
														<xsl:value-of select="type"/>
														<xsl:text>&amp;&gt;(</xsl:text>
														<xsl:value-of select="name"/>
														<xsl:text>)</xsl:text>
														<xsl:if test="position()!=last()">
															<xsl:text>, </xsl:text>
														</xsl:if>
													</xsl:when>
													<!-- parameter passed by value -->
													<xsl:when test="@passedBy='value'">
														<xsl:text>reinterpret_cast&lt;</xsl:text>
														<xsl:value-of select="../../../../@name"/>
														<xsl:text>::</xsl:text>
														<xsl:value-of select="type"/>
														<xsl:text>&gt;(</xsl:text>
														<xsl:value-of select="name"/>
														<xsl:text>)</xsl:text>
														<xsl:if test="position()!=last()">
															<xsl:text>, </xsl:text>
														</xsl:if>
													</xsl:when>
												</xsl:choose>
											</xsl:for-each>
											<xsl:text>);&#10;</xsl:text>
											<xsl:text>	return reinterpret_cast&lt;jlong&gt;(ptr</xsl:text>
											<xsl:value-of select="type"/>
											<xsl:text>);&#10;</xsl:text>
										</xsl:when>
										<!-- function type not(simpleTypes) (by value) -->
										<xsl:otherwise>
											<!-- calling the toEnum function for enum parameters -->
											<xsl:for-each select="parameters/parameter">
												<xsl:if test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
													<xsl:text>	</xsl:text>
													<xsl:value-of select="$namespace"/>
													<xsl:text>::</xsl:text>
													<xsl:choose>
														<xsl:when test="contains(type,'.')">
															<xsl:value-of select="substring-before(type,'.')"/>
															<xsl:text>::</xsl:text>
															<xsl:value-of select="substring-after(type,'.')"/>
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="type"/>
														</xsl:otherwise>
													</xsl:choose>
													<xsl:text> </xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>Enum = Ogre4J::to</xsl:text>
													<xsl:value-of select="substring-after(type,'.')"/>
													<xsl:text>Enum(</xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>);&#10;</xsl:text>
												</xsl:if>
												<!-- parameter is a string -->
												<xsl:if test="type='String'">
													<xsl:text>	</xsl:text>
													<xsl:value-of select="../../../../@name"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text> s</xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>;&#10;</xsl:text>
													<xsl:text>	</xsl:text>
													<xsl:text>Ogre4JHelper::CopyJString( env, </xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>, s</xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>);&#10;</xsl:text>
												</xsl:if>
											</xsl:for-each>
											<xsl:choose>
												<xsl:when test="@passedBy='value'">
													<xsl:text>	</xsl:text>
													<xsl:value-of select="$namespace"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text> p</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text> = p</xsl:text>
													<xsl:value-of select="$class"/>
													<xsl:text>-></xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>(</xsl:text>
													<xsl:for-each select="parameters/parameter">
														<xsl:value-of select="name"/>
														<xsl:choose>
															<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
																<xsl:text>Enum</xsl:text>
															</xsl:when>
															<xsl:when test="type='boolean'">
																<xsl:text>?true:false</xsl:text>
															</xsl:when>
														</xsl:choose>
														<xsl:if test="position()!=last()">
															<xsl:text>, </xsl:text>
														</xsl:if>
													</xsl:for-each>
													<xsl:text>);&#10;</xsl:text>
													<xsl:text>	</xsl:text>
													<xsl:value-of select="$namespace"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text> * p</xsl:text>
													<xsl:value-of select="$class"/>
													<xsl:text>2 = new </xsl:text>
													<xsl:value-of select="$namespace"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text>(p</xsl:text>
													<xsl:value-of select="$class"/>
													<xsl:text>);&#10;</xsl:text>
													<xsl:text>	return reinterpret_cast&lt;jlong&gt;(&amp;p</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text>2);&#10;</xsl:text>
												</xsl:when>
												<xsl:otherwise>
													<xsl:text>	</xsl:text>
													<xsl:value-of select="$namespace"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text> p</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text> = p</xsl:text>
													<xsl:value-of select="$class"/>
													<xsl:text>-></xsl:text>
													<xsl:value-of select="name"/>
													<xsl:text>(</xsl:text>
													<xsl:for-each select="parameters/parameter">
														<xsl:value-of select="name"/>
														<xsl:choose>
															<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
																<xsl:text>Enum</xsl:text>
															</xsl:when>
															<xsl:when test="type='boolean'">
																<xsl:text>?true:false</xsl:text>
															</xsl:when>
														</xsl:choose>
														<xsl:if test="position()!=last()">
															<xsl:text>, </xsl:text>
														</xsl:if>
													</xsl:for-each>
													<xsl:text>);&#10;</xsl:text>
													<xsl:text>	</xsl:text>
													<xsl:value-of select="$namespace"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text> * p</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text>2 = new </xsl:text>
													<xsl:value-of select="$namespace"/>
													<xsl:text>::</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text>(p</xsl:text>
													<xsl:value-of select="$class"/>
													<xsl:text>);&#10;</xsl:text>
													<xsl:text>	return reinterpret_cast&lt;jlong&gt;(&amp;p</xsl:text>
													<xsl:value-of select="type"/>
													<xsl:text>2);&#10;</xsl:text>
													<!--
												<xsl:text>recheck this: &#10;</xsl:text>
												<xsl:text>	</xsl:text>
												<xsl:value-of select="../../@name"/>
												<xsl:text>::</xsl:text>
												<xsl:value-of select="../@name"/>
												<xsl:text> * p</xsl:text>
												<xsl:value-of select="../@name"/>
												<xsl:text>2 = (</xsl:text>
												<xsl:value-of select="../../@name"/>
												<xsl:text>::</xsl:text>
												<xsl:value-of select="../@name"/>
												<xsl:text>*)(p</xsl:text>
												<xsl:value-of select="parameters/parameter/name"/>
												<xsl:text>);&#10;</xsl:text>
												<xsl:text>	</xsl:text>
												<xsl:value-of select="../../@name"/>
												<xsl:text>::</xsl:text>
												<xsl:value-of select="../@name"/>
												<xsl:text> * </xsl:text>
												<xsl:value-of select="name"/>
												<xsl:text> = new </xsl:text>
												<xsl:value-of select="../../@name"/>
												<xsl:text>::</xsl:text>
												<xsl:value-of select="../@name"/>
												<xsl:text>(p</xsl:text>
												<xsl:value-of select="../@name"/>
												<xsl:text>-></xsl:text>
												<xsl:value-of select="name"/>
												<xsl:text>(*p</xsl:text>
												<xsl:value-of select="../@name"/>
												<xsl:text>2));&#10;</xsl:text>
												<xsl:text>	return reinterpret_cast&lt;jint*&gt;(</xsl:text>
												<xsl:value-of select="name"/>
												<xsl:text>);&#10;</xsl:text>
												-->
												</xsl:otherwise>
											</xsl:choose>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<!-- not used anymore -->
								<xsl:otherwise>
									<xsl:text>###return reinterpret_cast&lt;jint*&gt;(p</xsl:text>
									<xsl:value-of select="type"/>
									<xsl:text>);</xsl:text>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<!-- ### any return value, no parameters ### -->
						<xsl:when test="type">
							<xsl:choose>
								<!-- return value is a string -->
								<xsl:when test="type='String'">
									<xsl:text>	return env ->NewStringUTF(p</xsl:text>
									<xsl:value-of select="../@name"/>
									<xsl:text>-></xsl:text>
									<xsl:value-of select="name"/>
									<xsl:text>().c_str());&#10;</xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<xsl:text>	return p</xsl:text>
									<xsl:value-of select="../@name"/>
									<xsl:text>-></xsl:text>
									<xsl:value-of select="name"/>
									<xsl:text>();&#10;</xsl:text>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<!-- not implemented ;-) -->
						<xsl:otherwise>
							<xsl:text>not implemented yet</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
					<!-- exception handling, catch block -->
					<xsl:choose>
						<xsl:when test="$config/config/exception_handling/@use='0'"/>
						<xsl:when test="$config/config/exception_handling/@use='1'">
							<xsl:text>&#10;</xsl:text>
							<xsl:text>	}&#10;</xsl:text>
							<xsl:text>	catch( Ogre::Exception &amp; e ) {&#10;</xsl:text>
							<xsl:text>		jclass OgreException = env->FindClass( "org/ogre4j/OgreException" );&#10;</xsl:text>
							<xsl:text>		env->ThrowNew(OgreException, e.getFullDescription().c_str());&#10;</xsl:text>
							<xsl:text>	}</xsl:text>
						</xsl:when>
						<xsl:when test="$config/config/exception_handling/@use='2'"/>
						<xsl:otherwise>unhandeled usage option in	file: cpp2java.cfg.xslt	node: config/exception_handling/@use</xsl:otherwise>
					</xsl:choose>
					<xsl:text>}&#10;</xsl:text>
				</xsl:if>
			</xsl:for-each>
			<!-- dispose() -->
			<xsl:text>&#10;</xsl:text>
			<xsl:text>JNIEXPORT void JNICALL Java_org_ogre4j_</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>_dispose(JNIEnv *env, jobject that, jint pInstance) {&#10;</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:value-of select="../@name"/>
			<xsl:text>::</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text> * p</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text> = reinterpret_cast&lt;</xsl:text>
			<xsl:value-of select="../@name"/>
			<xsl:text>::</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>*&gt;(pInstance);&#10;</xsl:text>
			<xsl:text>	delete p</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>;&#10;</xsl:text>
			<xsl:text>}&#10;</xsl:text>
		</xsl:result-document>
	</xsl:template>
	<!-- #################################### FUNCTION, für .java ######################################## -->
	<xsl:template name="function">
		<xsl:param name="isTemplate"/>
		<xsl:param name="currentClass"/>
		<xsl:param name="typedefType"/>
		<xsl:if test="not(contains(name,'operator')) and not(starts-with(type,'OGRE'))">
			<xsl:choose>
				<!-- enumerations -->
				<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
					<xsl:text>	public </xsl:text>
					<!-- _Constants -->
					<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
						<xsl:value-of select="$config/config/constants"/>
						<xsl:text>.</xsl:text>
					</xsl:if>
					<xsl:value-of select="type"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="name"/>
					<xsl:text>(</xsl:text>
					<xsl:for-each select="parameters/parameter">
						<xsl:if test="name">
							<xsl:choose>
								<xsl:when test="$isTemplate='true' and type='T'">
									<xsl:call-template name="templateTypeMap">
										<xsl:with-param name="type">
											<xsl:for-each select="root()/meta/namespace/class[@name=$currentClass]/inherits/baseClass">
												<xsl:if test="contains(current(),'&lt;')">
													<xsl:value-of select="current()"/>
												</xsl:if>
											</xsl:for-each>
										</xsl:with-param>
									</xsl:call-template>
								</xsl:when>
								<xsl:otherwise>
									<!-- _Constants -->
									<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
										<xsl:value-of select="$config/config/constants"/>
										<xsl:text>.</xsl:text>
									</xsl:if>
									<xsl:call-template name="type">
										<xsl:with-param name="isParameter" select="'true'"/>
									</xsl:call-template>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:text> </xsl:text>
							<xsl:value-of select="name"/>
							<xsl:if test="position()!=last()">
								<xsl:text>, </xsl:text>
							</xsl:if>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>) {&#10;</xsl:text>
					<xsl:text>		return </xsl:text>
					<!-- _Constants -->
					<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
						<xsl:value-of select="$config/config/constants"/>
						<xsl:text>.</xsl:text>
					</xsl:if>
					<xsl:value-of select="type"/>
					<xsl:text>.toEnum(</xsl:text>
					<xsl:text>_</xsl:text>
					<xsl:value-of select="name"/>
					<xsl:call-template name="nativeFuncSignature"/>
					<xsl:text>(pInstance.value</xsl:text>
					<xsl:if test="parameters/parameter/name">
						<xsl:text>, </xsl:text>
					</xsl:if>
					<xsl:for-each select="parameters/parameter">
						<xsl:if test="name">
							<xsl:value-of select="name"/>
							<xsl:choose>
								<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
									<xsl:text>.value</xsl:text>
								</xsl:when>
								<xsl:when test="not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type)">
									<xsl:text>.pInstance.value</xsl:text>
								</xsl:when>
							</xsl:choose>
							<xsl:if test="position()!=last()">
								<xsl:text>, </xsl:text>
							</xsl:if>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>));&#10;</xsl:text>
					<xsl:text>	}&#10;</xsl:text>
				</xsl:when>
				<!-- function type not(simpleTypes), developer must pass object as parameter because function return value is passed by value -->
				<xsl:when test="(type!='T.pointer'  and type !='T.const_pointer') and (type and type!='void' and not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type) and @passedBy='value') or ( type=root()//typedef/@name and not(type=$typedefResultTree/mapping/@type) and type!='String')">
					<!-- function declaration -->
					<xsl:text>	public void </xsl:text>
					<xsl:value-of select="name"/>
					<xsl:if test="@const='true'">
						<xsl:text>_const</xsl:text>
					</xsl:if>
					<xsl:text>(</xsl:text>
					<xsl:if test="type!='void'">
						<xsl:choose>
							<xsl:when test="type='T.mapped_type'">
								<xsl:variable name="reference" select="$typedefType"/>
								<xsl:for-each select="root()//typedef[@name=$reference]">
									<xsl:if test="position()=1">
										<xsl:for-each select="type[position()=2]">
											<xsl:choose>
												<xsl:when test="current()=$typedefResultTree/mapping/@type">
													<xsl:call-template name="templateTypeMap">
														<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
													</xsl:call-template>
												</xsl:when>
												<xsl:otherwise>
													<xsl:call-template name="templateTypeMap">
														<xsl:with-param name="type" select="current()"/>
													</xsl:call-template>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:for-each>
									</xsl:if>
								</xsl:for-each>
							</xsl:when>
							<xsl:when test="type='T.key_type'">
								<xsl:variable name="reference" select="$typedefType"/>
								<xsl:for-each select="root()//typedef[@name=$reference]">
									<xsl:if test="position()=1">
										<xsl:for-each select="type[position()=1]">
											<xsl:choose>
												<xsl:when test="current()=$typedefResultTree/mapping/@type">
													<xsl:call-template name="templateTypeMap">
														<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
													</xsl:call-template>
												</xsl:when>
												<xsl:otherwise>
													<xsl:call-template name="templateTypeMap">
														<xsl:with-param name="type" select="current()"/>
													</xsl:call-template>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:for-each>
									</xsl:if>
								</xsl:for-each>
							</xsl:when>
							<xsl:when test="type='T.value_type'">
								<xsl:variable name="reference" select="$typedefType"/>
								<xsl:for-each select="root()//typedef[@name=$reference]">
									<xsl:if test="position()=1">
										<xsl:for-each select="type[position()=1]">
											<xsl:choose>
												<xsl:when test="current()=$typedefResultTree/mapping/@type">
													<xsl:call-template name="templateTypeMap">
														<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
													</xsl:call-template>
												</xsl:when>
												<xsl:otherwise>
													<xsl:call-template name="templateTypeMap">
														<xsl:with-param name="type" select="current()"/>
													</xsl:call-template>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:for-each>
									</xsl:if>
								</xsl:for-each>
							</xsl:when>
							<xsl:when test="$isTemplate='true' and type='T'">
								<xsl:call-template name="templateTypeMap">
									<xsl:with-param name="type">
										<xsl:for-each select="root()/meta/namespace/class[@name=$currentClass]/inherits/baseClass">
											<xsl:if test="contains(current(),'&lt;')">
												<xsl:value-of select="current()"/>
											</xsl:if>
										</xsl:for-each>
									</xsl:with-param>
								</xsl:call-template>
							</xsl:when>
							<xsl:otherwise>
								<!-- _Constants -->
								<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
									<xsl:value-of select="$config/config/constants"/>
									<xsl:text>.</xsl:text>
								</xsl:if>
								<xsl:call-template name="type"/>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:text> new</xsl:text>
						<xsl:choose>
							<xsl:when test="starts-with(type,'std.map')">
								<xsl:text>Map</xsl:text>
							</xsl:when>
							<xsl:when test="starts-with(type,'std.pair')">
								<xsl:text>MapEntry</xsl:text>
							</xsl:when>
							<xsl:when test="contains(type,'&lt;')">
								<xsl:value-of select="substring-before(type,'&lt;')"/>
							</xsl:when>
							<xsl:when test="contains(type,'.')">
								<xsl:value-of select="substring-after(type,'.')"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="type"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:if>
					<xsl:if test="parameters/parameter/type!='void'">
						<xsl:text>, </xsl:text>
						<xsl:for-each select="parameters/parameter">
							<xsl:choose>
								<xsl:when test="$isTemplate='true' and type='T'">
									<xsl:call-template name="templateTypeMap">
										<xsl:with-param name="type">
											<xsl:for-each select="root()/meta/namespace/class[@name=$currentClass]/inherits/baseClass">
												<xsl:if test="contains(current(),'&lt;')">
													<xsl:value-of select="current()"/>
												</xsl:if>
											</xsl:for-each>
										</xsl:with-param>
									</xsl:call-template>
								</xsl:when>
								<xsl:otherwise>
									<!-- _Constants -->
									<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
										<xsl:value-of select="$config/config/constants"/>
										<xsl:text>.</xsl:text>
									</xsl:if>
									<xsl:call-template name="type">
										<xsl:with-param name="isParameter" select="'true'"/>
									</xsl:call-template>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:text> </xsl:text>
							<xsl:value-of select="name"/>
							<xsl:if test="position()!=last()">
								<xsl:text>, </xsl:text>
							</xsl:if>
						</xsl:for-each>
					</xsl:if>
					<xsl:text>)</xsl:text>
					<xsl:choose>
						<xsl:when test="../@template='true' and $isTemplate!='true'">
							<xsl:text>;&#10;</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text> {&#10;</xsl:text>
							<!-- function implementation -->
							<xsl:if test="$config/config/validation/@use='true' and $config/config/autodispose/@use='true'">
								<xsl:text>		// validation == true &amp;&amp; autodispose==true&#10;</xsl:text>
								<xsl:text>		if(new</xsl:text>
								<xsl:choose>
									<xsl:when test="starts-with(type,'std.map')">
										<xsl:text>Map</xsl:text>
									</xsl:when>
									<xsl:when test="starts-with(type,'std.pair')">
										<xsl:text>MapEntry</xsl:text>
									</xsl:when>
									<xsl:when test="contains(type,'&lt;')">
										<xsl:value-of select="substring-before(type,'&lt;')"/>
									</xsl:when>
									<xsl:when test="contains(type,'.')">
										<xsl:value-of select="substring-after(type,'.')"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="type"/>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:text> == null)&#10;</xsl:text>
								<xsl:text>			throw new NullPointerException("object as parameter expected");&#10;</xsl:text>
								<!-- no autodispose if global enum -->
								<xsl:if test="type=root()/meta/namespace/enum/@name">
									<xsl:text>		if (new</xsl:text>
									<xsl:choose>
										<xsl:when test="starts-with(type,'std.map')">
											<xsl:text>Map</xsl:text>
										</xsl:when>
										<xsl:when test="starts-with(type,'std.pair')">
											<xsl:text>MapEntry</xsl:text>
										</xsl:when>
										<xsl:when test="contains(type,'&lt;')">
											<xsl:value-of select="substring-before(type,'&lt;')"/>
										</xsl:when>
										<xsl:when test="contains(type,'.')">
											<xsl:value-of select="substring-after(type,'.')"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="type"/>
										</xsl:otherwise>
									</xsl:choose>
									<xsl:text>.pInstance != null) new</xsl:text>
									<xsl:value-of select="type"/>
									<xsl:text>.dispose();&#10;</xsl:text>
								</xsl:if>
								<xsl:text>&#10;</xsl:text>
							</xsl:if>
							<xsl:if test="$config/config/validation/@use='true' and $config/config/autodispose/@use='false'">
								<xsl:text>		// validation == true &amp;&amp; autodispose==false&#10;</xsl:text>
								<xsl:text>		if(new</xsl:text>
								<xsl:choose>
									<xsl:when test="starts-with(type,'std.map')">
										<xsl:text>Map</xsl:text>
									</xsl:when>
									<xsl:when test="starts-with(type,'std.pair')">
										<xsl:text>MapEntry</xsl:text>
									</xsl:when>
									<xsl:when test="contains(type,'&lt;')">
										<xsl:value-of select="substring-before(type,'&lt;')"/>
									</xsl:when>
									<xsl:when test="contains(type,'.')">
										<xsl:value-of select="substring-after(type,'.')"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="type"/>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:text> == null || new</xsl:text>
								<xsl:value-of select="type"/>
								<xsl:text>.pInstance != null )&#10;</xsl:text>
								<xsl:text>			throw new NullPointerException("object as parameter expected");&#10;</xsl:text>
								<xsl:text>&#10;</xsl:text>
							</xsl:if>
							<xsl:if test="$config/config/validation/@use='false' and $config/config/autodispose/@use='true'">
								<xsl:text>		// validation == false &amp;&amp; autodispose==true&#10;</xsl:text>
								<!-- no autodispose if global enum -->
								<xsl:if test="type=root()/meta/namespace/enum/@name">
									<xsl:text>		if(new</xsl:text>
									<xsl:choose>
										<xsl:when test="starts-with(type,'std.map')">
											<xsl:text>Map</xsl:text>
										</xsl:when>
										<xsl:when test="starts-with(type,'std.pair')">
											<xsl:text>MapEntry</xsl:text>
										</xsl:when>
										<xsl:when test="contains(type,'&lt;')">
											<xsl:value-of select="substring-before(type,'&lt;')"/>
										</xsl:when>
										<xsl:when test="contains(type,'.')">
											<xsl:value-of select="substring-after(type,'.')"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="type"/>
										</xsl:otherwise>
									</xsl:choose>
									<xsl:text> != null &amp;&amp; new</xsl:text>
									<xsl:value-of select="type"/>
									<xsl:text>.pInstance != null )&#10;</xsl:text>
									<xsl:text>			new</xsl:text>
									<xsl:choose>
										<xsl:when test="starts-with(type,'std.map')">
											<xsl:text>Map</xsl:text>
										</xsl:when>
										<xsl:when test="starts-with(type,'std.pair')">
											<xsl:text>MapEntry</xsl:text>
										</xsl:when>
										<xsl:when test="contains(type,'&lt;')">
											<xsl:value-of select="substring-before(type,'&lt;')"/>
										</xsl:when>
										<xsl:when test="contains(type,'.')">
											<xsl:value-of select="substring-after(type,'.')"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="type"/>
										</xsl:otherwise>
									</xsl:choose>
									<xsl:text>.dispose();&#10;</xsl:text>
								</xsl:if>
								<xsl:text>&#10;</xsl:text>
							</xsl:if>
							<xsl:text>		long ptr = _</xsl:text>
							<xsl:value-of select="name"/>
							<xsl:call-template name="nativeFuncSignature"/>
							<xsl:if test="@const='true'">
								<xsl:text>_const</xsl:text>
							</xsl:if>
							<xsl:text>(pInstance.value</xsl:text>
							<xsl:if test="parameters/parameter/type!='void'">
								<xsl:for-each select="parameters/parameter">
									<xsl:text>, </xsl:text>
									<xsl:value-of select="name"/>
									<xsl:choose>
										<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
											<xsl:text>.value</xsl:text>
										</xsl:when>
										<xsl:when test="not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type)">
											<xsl:text>.pInstance.value</xsl:text>
										</xsl:when>
									</xsl:choose>
								</xsl:for-each>
							</xsl:if>
							<xsl:text>);&#10;</xsl:text>
							<xsl:text>		if(ptr==0) throw new NullPointerException("error in JNI call");&#10;</xsl:text>
							<!-- no autodispose if enum -->
							<xsl:if test="type=root()/meta/namespace/enum/@name">
								<xsl:text>		new</xsl:text>
								<xsl:choose>
									<xsl:when test="starts-with(type,'std.map')">
										<xsl:text>Map</xsl:text>
									</xsl:when>
									<xsl:when test="starts-with(type,'std.pair')">
										<xsl:text>MapEntry</xsl:text>
									</xsl:when>
									<xsl:when test="contains(type,'&lt;')">
										<xsl:value-of select="substring-before(type,'&lt;')"/>
									</xsl:when>
									<xsl:when test="contains(type,'.')">
										<xsl:value-of select="substring-after(type,'.')"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="type"/>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:text>.pInstance = new InstancePointer(ptr);&#10;</xsl:text>
							</xsl:if>
							<xsl:text>	}&#10;</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<!-- ############################################################## -->
				<!-- normal functions -->
				<xsl:otherwise>
					<!-- function declaration -->
					<xsl:if test="type or not(../@template='true')">
						<xsl:text>	public </xsl:text>
						<xsl:choose>
							<xsl:when test="type='T.pointer' or type='T.const_pointer'">
								<xsl:variable name="reference" select="$typedefType"/>
								<xsl:choose>
									<!-- typedef has 2 type elements -->
									<xsl:when test="root()//typedef[@name=$reference]/type[position()=2]">
										<xsl:for-each select="root()//typedef[@name=$reference]">
											<xsl:if test="position()=1">
												<xsl:for-each select="type[position()=2]">
													<xsl:choose>
														<xsl:when test="current()=$typedefResultTree/mapping/@type">
															<xsl:call-template name="templateTypeMap">
																<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
															</xsl:call-template>
														</xsl:when>
														<xsl:otherwise>
															<xsl:call-template name="templateTypeMap">
																<xsl:with-param name="type" select="current()"/>
															</xsl:call-template>
														</xsl:otherwise>
													</xsl:choose>
												</xsl:for-each>
											</xsl:if>
										</xsl:for-each>
									</xsl:when>
									<!-- typedef has 1 type element -->
									<xsl:otherwise>
										<xsl:for-each select="root()//typedef[@name=$reference]">
											<xsl:if test="position()=1">
												<xsl:for-each select="type[position()=1]">
													<xsl:choose>
														<xsl:when test="current()=$typedefResultTree/mapping/@type">
															<xsl:call-template name="templateTypeMap">
																<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
															</xsl:call-template>
														</xsl:when>
														<xsl:otherwise>
															<xsl:call-template name="templateTypeMap">
																<xsl:with-param name="type" select="current()"/>
															</xsl:call-template>
														</xsl:otherwise>
													</xsl:choose>
												</xsl:for-each>
											</xsl:if>
										</xsl:for-each>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:when test="$isTemplate='true' and type='T'">
								<xsl:value-of select="$typedefType"/>
								<xsl:call-template name="templateTypeMap">
									<xsl:with-param name="type">
										<xsl:for-each select="root()/meta/namespace/class[@name=$currentClass]/inherits/baseClass">
											<xsl:if test="contains(current(),'&lt;')">
												<xsl:value-of select="current()"/>
											</xsl:if>
										</xsl:for-each>
									</xsl:with-param>
								</xsl:call-template>
							</xsl:when>
							<xsl:otherwise>
								<!-- _Constants -->
								<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
									<xsl:value-of select="$config/config/constants"/>
									<xsl:text>.</xsl:text>
								</xsl:if>
								<xsl:call-template name="type"/>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:text> </xsl:text>
						<xsl:value-of select="name"/>
						<xsl:if test="@const='true'">
							<xsl:text>_const</xsl:text>
						</xsl:if>
						<xsl:text>(</xsl:text>
						<xsl:if test="parameters/parameter/name or (parameters/parameter/type and parameters/parameter/type!='void')">
							<xsl:for-each select="parameters/parameter">
								<xsl:choose>
									<xsl:when test="$isTemplate='true' and type='T'">
										<xsl:value-of select="$typedefType"/>
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type">
												<xsl:for-each select="root()/meta/namespace/class[@name=$currentClass]/inherits/baseClass">
													<xsl:if test="contains(current(),'&lt;')">
														<xsl:value-of select="current()"/>
													</xsl:if>
												</xsl:for-each>
											</xsl:with-param>
										</xsl:call-template>
									</xsl:when>
									<xsl:otherwise>
										<!-- _Constants -->
										<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
											<xsl:value-of select="$config/config/constants"/>
											<xsl:text>.</xsl:text>
										</xsl:if>
										<xsl:call-template name="type">
											<xsl:with-param name="isParameter" select="'true'"/>
										</xsl:call-template>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:text> </xsl:text>
								<xsl:choose>
									<xsl:when test="name">
										<xsl:value-of select="name"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:text>var</xsl:text>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:if test="position()!=last()">
									<xsl:text>, </xsl:text>
								</xsl:if>
							</xsl:for-each>
						</xsl:if>
						<xsl:text>)</xsl:text>
						<xsl:choose>
							<xsl:when test="../@template='true' and $isTemplate!='true'">
								<xsl:text>;&#10;</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text> {&#10;</xsl:text>
								<!-- function implementation -->
								<xsl:text>		</xsl:text>
								<xsl:choose>
									<!-- any return value, with parameters, function passedBy pointer/reference -->
									<!--									<xsl:when test="type and type!='void' and not(contains($simpleTypes,type)) and parameters and (@passedBy='pointer' or @passedBy='reference')">-->
									<xsl:when test="type and type!='void' and not(contains($simpleTypes,type)) and (@passedBy='pointer' or @passedBy='reference') or type ='T.pointer' or type ='T.const_pointer'">
										<xsl:text>long ptr = _</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:call-template name="nativeFuncSignature"/>
										<xsl:if test="@const='true'">
											<xsl:text>_const</xsl:text>
										</xsl:if>
										<xsl:text>(pInstance.value</xsl:text>
										<xsl:if test="parameters/parameter/type!='void'">
											<xsl:for-each select="parameters/parameter">
												<xsl:text>, </xsl:text>
												<xsl:value-of select="name"/>
												<xsl:choose>
													<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
														<xsl:text>.value</xsl:text>
													</xsl:when>
													<xsl:when test="not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type)">
														<xsl:text>.pInstance.value</xsl:text>
													</xsl:when>
												</xsl:choose>
											</xsl:for-each>
										</xsl:if>
										<xsl:text>);&#10;</xsl:text>
										<xsl:text>		if(ptr==0) return null;&#10;</xsl:text>
										<xsl:text>		InstancePointer ptr</xsl:text>
										<xsl:choose>
											<xsl:when test="type='T.pointer' or type='T.const_pointer'">
												<xsl:variable name="reference" select="$typedefType"/>
												<xsl:choose>
													<!-- typedef has 2 type elements -->
													<xsl:when test="root()//typedef[@name=$reference]/type[position()=2]">
														<xsl:for-each select="root()//typedef[@name=$reference]">
															<xsl:if test="position()=1">
																<xsl:for-each select="type[position()=2]">
																	<xsl:choose>
																		<xsl:when test="current()=$typedefResultTree/mapping/@type">
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
																			</xsl:call-template>
																		</xsl:when>
																		<xsl:otherwise>
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="current()"/>
																			</xsl:call-template>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:for-each>
															</xsl:if>
														</xsl:for-each>
													</xsl:when>
													<!-- typedef has 1 type element -->
													<xsl:otherwise>
														<xsl:for-each select="root()//typedef[@name=$reference]">
															<xsl:if test="position()=1">
																<xsl:for-each select="type[position()=1]">
																	<xsl:choose>
																		<xsl:when test="current()=$typedefResultTree/mapping/@type">
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
																			</xsl:call-template>
																		</xsl:when>
																		<xsl:otherwise>
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="current()"/>
																			</xsl:call-template>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:for-each>
															</xsl:if>
														</xsl:for-each>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:when>
											<xsl:otherwise>
												<xsl:value-of select="type"/>
											</xsl:otherwise>
										</xsl:choose>
										<xsl:text> = new  InstancePointer(ptr);&#10;</xsl:text>
										<xsl:text>		return new </xsl:text>
										<xsl:choose>
											<xsl:when test="type='T.pointer' or type='T.const_pointer'">
												<xsl:variable name="reference" select="$typedefType"/>
												<xsl:choose>
													<!-- typedef has 2 type elements -->
													<xsl:when test="root()//typedef[@name=$reference]/type[position()=2]">
														<xsl:for-each select="root()//typedef[@name=$reference]">
															<xsl:if test="position()=1">
																<xsl:for-each select="type[position()=2]">
																	<xsl:choose>
																		<xsl:when test="current()=$typedefResultTree/mapping/@type">
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
																			</xsl:call-template>
																		</xsl:when>
																		<xsl:otherwise>
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="current()"/>
																			</xsl:call-template>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:for-each>
															</xsl:if>
														</xsl:for-each>
													</xsl:when>
													<!-- typedef has 1 type element -->
													<xsl:otherwise>
														<xsl:for-each select="root()//typedef[@name=$reference]">
															<xsl:if test="position()=1">
																<xsl:for-each select="type[position()=1]">
																	<xsl:choose>
																		<xsl:when test="current()=$typedefResultTree/mapping/@type">
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
																			</xsl:call-template>
																		</xsl:when>
																		<xsl:otherwise>
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="current()"/>
																			</xsl:call-template>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:for-each>
															</xsl:if>
														</xsl:for-each>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:when>
											<xsl:when test="$isTemplate='true' and type='T'">
												<xsl:value-of select="$typedefType"/>
												<xsl:call-template name="templateTypeMap">
													<xsl:with-param name="type">
														<xsl:for-each select="root()/meta/namespace/class[@name=$currentClass]/inherits/baseClass">
															<xsl:if test="contains(current(),'&lt;')">
																<xsl:value-of select="current()"/>
															</xsl:if>
														</xsl:for-each>
													</xsl:with-param>
												</xsl:call-template>
											</xsl:when>
											<xsl:otherwise>
												<!-- _Constants -->
												<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
													<xsl:value-of select="$config/config/constants"/>
													<xsl:text>.</xsl:text>
												</xsl:if>
												<xsl:call-template name="type"/>
											</xsl:otherwise>
										</xsl:choose>
										<xsl:text>(ptr</xsl:text>
										<xsl:choose>
											<xsl:when test="type='T.pointer' or type='T.const_pointer'">
												<xsl:variable name="reference" select="$typedefType"/>
												<xsl:choose>
													<!-- typedef has 2 type elements -->
													<xsl:when test="root()//typedef[@name=$reference]/type[position()=2]">
														<xsl:for-each select="root()//typedef[@name=$reference]">
															<xsl:if test="position()=1">
																<xsl:for-each select="type[position()=2]">
																	<xsl:choose>
																		<xsl:when test="current()=$typedefResultTree/mapping/@type">
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
																			</xsl:call-template>
																		</xsl:when>
																		<xsl:otherwise>
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="current()"/>
																			</xsl:call-template>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:for-each>
															</xsl:if>
														</xsl:for-each>
													</xsl:when>
													<!-- typedef has 1 type element -->
													<xsl:otherwise>
														<xsl:for-each select="root()//typedef[@name=$reference]">
															<xsl:if test="position()=1">
																<xsl:for-each select="type[position()=1]">
																	<xsl:choose>
																		<xsl:when test="current()=$typedefResultTree/mapping/@type">
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
																			</xsl:call-template>
																		</xsl:when>
																		<xsl:otherwise>
																			<xsl:call-template name="templateTypeMap">
																				<xsl:with-param name="type" select="current()"/>
																			</xsl:call-template>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:for-each>
															</xsl:if>
														</xsl:for-each>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:when>
											<xsl:otherwise>
												<xsl:value-of select="type"/>
											</xsl:otherwise>
										</xsl:choose>
										<xsl:text>);</xsl:text>
									</xsl:when>
									<!-- any return value, with parameters -->
									<xsl:when test="type and type!='void' and parameters/parameter/name">
										<xsl:text>return _</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:call-template name="nativeFuncSignature"/>
										<xsl:if test="@const='true'">
											<xsl:text>_const</xsl:text>
										</xsl:if>
										<xsl:text>(pInstance.value</xsl:text>
										<xsl:if test="parameters/parameter/name">
											<xsl:text>, </xsl:text>
										</xsl:if>
										<xsl:for-each select="parameters/parameter">
											<xsl:value-of select="name"/>
											<xsl:choose>
												<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
													<xsl:text>.value</xsl:text>
												</xsl:when>
												<xsl:when test="not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type)">
													<xsl:text>.pInstance.value</xsl:text>
												</xsl:when>
											</xsl:choose>
											<xsl:if test="position()!=last()">
												<xsl:text>, </xsl:text>
											</xsl:if>
										</xsl:for-each>
										<xsl:text>);</xsl:text>
									</xsl:when>
									<!-- constructor -->
									<xsl:when test="name=../@name">
										<!--<xsl:if test="../@template!='true'">-->
										<xsl:choose>
											<!-- parameter is Vector2/3/4 -->
											<!-- ToDo: more vecmath -->
											<xsl:when test="(parameters/parameter/type='Vector2' or parameters/parameter/type='Vector3' or parameters/parameter/type='Vector4') and $config/config/vecmath='true'">
												<xsl:text>this(min.x, min.y, min.z, max.x, max.y, max.z);</xsl:text>
											</xsl:when>
											<xsl:when test="parameters">
												<!-- no super() in inner class -->
												<xsl:choose>
													<xsl:when test="../../../class">
														<xsl:text>this.pInstance = new InstancePointer( _createInstance</xsl:text>
														<xsl:call-template name="nativeFuncSignature"/>
														<xsl:if test="@const='true'">
															<xsl:text>_const</xsl:text>
														</xsl:if>
														<xsl:text>(</xsl:text>
													</xsl:when>
													<xsl:otherwise>
														<xsl:text>super( new InstancePointer( _createInstance</xsl:text>
														<xsl:call-template name="nativeFuncSignature"/>
														<xsl:if test="@const='true'">
															<xsl:text>_const</xsl:text>
														</xsl:if>
														<xsl:text>(</xsl:text>
													</xsl:otherwise>
												</xsl:choose>
												<xsl:for-each select="parameters/parameter">
													<xsl:value-of select="name"/>
													<xsl:choose>
														<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
															<xsl:text>.value</xsl:text>
														</xsl:when>
														<xsl:when test="not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type)">
															<xsl:text>.pInstance.value</xsl:text>
														</xsl:when>
													</xsl:choose>
													<xsl:if test="position()!=last()">
														<xsl:text>, </xsl:text>
													</xsl:if>
												</xsl:for-each>
												<xsl:choose>
													<xsl:when test="../../../class">
														<xsl:text>));</xsl:text>
													</xsl:when>
													<xsl:otherwise>
														<xsl:text>)));</xsl:text>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:when>
											<!-- default construktor -->
											<xsl:otherwise>
												<!-- no super() in inner class -->
												<xsl:choose>
													<xsl:when test="../../../class">
														<xsl:text>this.pInstance = new InstancePointer( _createInstance</xsl:text>
														<xsl:call-template name="nativeFuncSignature"/>
														<xsl:if test="@const='true'">
															<xsl:text>_const</xsl:text>
														</xsl:if>
														<xsl:text>());</xsl:text>
													</xsl:when>
													<xsl:otherwise>
														<xsl:text>	super( new InstancePointer( _createInstance</xsl:text>
														<xsl:call-template name="nativeFuncSignature"/>
														<xsl:if test="@const='true'">
															<xsl:text>_const</xsl:text>
														</xsl:if>
														<xsl:text>()));</xsl:text>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:otherwise>
										</xsl:choose>
										<!--</xsl:if>-->
									</xsl:when>
									<!-- no return value (void)-->
									<xsl:when test="type='void'">
										<xsl:text>_</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:call-template name="nativeFuncSignature"/>
										<xsl:if test="@const='true'">
											<xsl:text>_const</xsl:text>
										</xsl:if>
										<xsl:text>(pInstance.value</xsl:text>
										<xsl:for-each select="parameters/parameter">
											<xsl:if test="name or (type and type!='void')">
												<xsl:text>, </xsl:text>
												<xsl:choose>
													<xsl:when test="name">
														<xsl:value-of select="name"/>
													</xsl:when>
													<xsl:otherwise>var</xsl:otherwise>
												</xsl:choose>
												<xsl:choose>
													<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
														<xsl:text>.value</xsl:text>
													</xsl:when>
													<xsl:when test="not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type)">
														<xsl:text>.pInstance.value</xsl:text>
													</xsl:when>
												</xsl:choose>
											</xsl:if>
										</xsl:for-each>
										<xsl:text>);</xsl:text>
									</xsl:when>
									<!-- return value Vector3, no parameters -->
									<!-- ToDo: more vecmath -->
									<xsl:when test="type='Vector3' and $config/config/vecmath='true'">
										<xsl:text>float[] tuple = _</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:call-template name="nativeFuncSignature"/>
										<xsl:if test="@const='true'">
											<xsl:text>_const</xsl:text>
										</xsl:if>
										<xsl:text>(pInstance.value);&#10;</xsl:text>
										<xsl:text>		return new Point3f(tuple);</xsl:text>
									</xsl:when>
									<!-- any return value, no parameter name -->
									<xsl:when test="type">
										<xsl:text>return _</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:call-template name="nativeFuncSignature"/>
										<xsl:if test="@const='true'">
											<xsl:text>_const</xsl:text>
										</xsl:if>
										<xsl:text>(pInstance.value</xsl:text>
										<xsl:for-each select="parameters/parameter">
											<xsl:if test="type and type!='void'">
												<xsl:text>, </xsl:text>
												<xsl:choose>
													<xsl:when test="name">
														<xsl:value-of select="name"/>
													</xsl:when>
													<xsl:otherwise>var</xsl:otherwise>
												</xsl:choose>
												<xsl:choose>
													<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
														<xsl:text>.value</xsl:text>
													</xsl:when>
													<xsl:when test="not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type)">
														<xsl:text>.pInstance.value</xsl:text>
													</xsl:when>
												</xsl:choose>
											</xsl:if>
										</xsl:for-each>
										<xsl:text>);</xsl:text>
									</xsl:when>
									<!-- constructor of embedded class -->
									<xsl:otherwise>
										<xsl:choose>
											<!-- ToDo: more vecmath -->
											<xsl:when test="parameters/parameter/type='Vector3' and $config/config/vecmath='true'">
												<xsl:text>this(min.x, min.y, min.z, max.x, max.y, max.z);</xsl:text>
											</xsl:when>
											<xsl:when test="parameters">
												<!-- no super() in inner class -->
												<xsl:choose>
													<xsl:when test="../../../class">
														<xsl:text>this.pInstance = new InstancePointer( _createInstance</xsl:text>
														<xsl:call-template name="nativeFuncSignature"/>
														<xsl:if test="@const='true'">
															<xsl:text>_const</xsl:text>
														</xsl:if>
														<xsl:text>(</xsl:text>
													</xsl:when>
													<xsl:otherwise>
														<xsl:text>super( new InstancePointer( _createInstance</xsl:text>
														<xsl:call-template name="nativeFuncSignature"/>
														<xsl:if test="@const='true'">
															<xsl:text>_const</xsl:text>
														</xsl:if>
														<xsl:text>(</xsl:text>
													</xsl:otherwise>
												</xsl:choose>
												<xsl:for-each select="parameters/parameter">
													<xsl:value-of select="name"/>
													<xsl:choose>
														<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
															<xsl:text>.value</xsl:text>
														</xsl:when>
														<xsl:when test="not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type)">
															<xsl:text>.pInstance.value</xsl:text>
														</xsl:when>
													</xsl:choose>
													<xsl:if test="position()!=last()">
														<xsl:text>, </xsl:text>
													</xsl:if>
												</xsl:for-each>
												<xsl:choose>
													<xsl:when test="../../../class">
														<xsl:text>));</xsl:text>
													</xsl:when>
													<xsl:otherwise>
														<xsl:text>)));</xsl:text>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:when>
											<xsl:otherwise>
												<xsl:text>super(pInstance);</xsl:text>
											</xsl:otherwise>
										</xsl:choose>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:text>&#10;</xsl:text>
								<!-- array length exception -->
								<xsl:for-each select="parameters/parameter">
									<xsl:if test="type/@array">
										<xsl:text>		if (</xsl:text>
										<xsl:value-of select="name"/>
										<xsl:text>.length != </xsl:text>
										<xsl:value-of select="translate(type/@array,'[]','')"/>
										<xsl:text>)&#10;</xsl:text>
										<xsl:text>			throw new RuntimeException("length incorrect");&#10;</xsl:text>
									</xsl:if>
								</xsl:for-each>
								<xsl:text>	}&#10;</xsl:text>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>
	<!-- #################################### NATIVEFUNCTION, für .java ######################################## -->
	<xsl:template name="nativeFunction">
		<xsl:param name="isTemplate"/>
		<xsl:param name="currentClass"/>
		<xsl:param name="calledFrom"/>
		<xsl:choose>
			<!-- enumerations -->
			<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
				<xsl:text>	private static native int _</xsl:text>
				<xsl:value-of select="name"/>
				<xsl:call-template name="nativeFuncSignature"/>
				<xsl:text>(long pInstance</xsl:text>
				<xsl:if test="parameters/parameter/type!='void'">
					<xsl:for-each select="parameters/parameter">
						<xsl:if test="name">
							<xsl:text>, </xsl:text>
							<xsl:choose>
								<xsl:when test="$isTemplate='true' and type='T'">
									<xsl:text>long</xsl:text>
								</xsl:when>
								<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
									<xsl:text>int</xsl:text>
								</xsl:when>
								<xsl:when test="type=root()/meta/namespace/typedef/@name and type!='String'">
									<xsl:text>long</xsl:text>
								</xsl:when>
								<xsl:when test="type='void'">
									<xsl:text>long</xsl:text>
								</xsl:when>
								<xsl:when test="not(contains($simpleTypes,type))">
									<xsl:text>long</xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<xsl:call-template name="type">
										<xsl:with-param name="isParameter" select="'true'"/>
									</xsl:call-template>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:text> </xsl:text>
							<xsl:value-of select="name"/>
						</xsl:if>
					</xsl:for-each>
				</xsl:if>
				<xsl:text>);</xsl:text>
				<xsl:text>&#10;</xsl:text>
			</xsl:when>
			<xsl:when test="name!=../@name and not(contains(name,'operator'))">
				<xsl:text>	private native </xsl:text>
				<xsl:choose>
					<!-- any return value, without parameters -->
					<!-- developer must pass object as parameter -->
					<xsl:when test="type and type!='void' and not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type)">
						<xsl:choose>
							<xsl:when test="($isTemplate='true' and type='T') or type=root()//class/@name or (type=root()//typedef/@name and not(type=$typedefResultTree/mapping/@type) and type!='String') or substring-after(type,'.')=root()//typedef/@name or type=root()//struct/@name or starts-with(type,'std.') or type='T.mapped_type' or type='T.key_type' or type='T.pointer' or type='T.const_pointer' or type='T.value_type'">
								<xsl:text>long</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:call-template name="type"/>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:text> _</xsl:text>
						<xsl:value-of select="name"/>
						<xsl:call-template name="nativeFuncSignature"/>
						<xsl:if test="@const='true'">
							<xsl:text>_const</xsl:text>
						</xsl:if>
						<xsl:text>(long pInstance</xsl:text>
						<!-- Objekt soll nicht übergeben werden, sondern von C++ zurückgegeben werden
						<xsl:if test="type!='void'">
							<xsl:text>, </xsl:text>
							<xsl:choose>
								<xsl:when test="$isTemplate='true' and type='T'">
									<xsl:call-template name="templateTypeMap">
										<xsl:with-param name="template" select="root()/meta/namespace/class[@name=$currentClass]/inherits/baseClass"/>
									</xsl:call-template>
								</xsl:when>
								<xsl:otherwise>-->
						<!-- _Constants -->
						<!--<xsl:if test="type=root()/meta/namespace/enumeration/@name">
										<xsl:value-of select="$config/config/constants"/>
										<xsl:text>.</xsl:text>
									</xsl:if>
									<xsl:value-of select="type"/>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:text> p</xsl:text>
							<xsl:value-of select="type"/>
						</xsl:if>-->
						<xsl:if test="parameters/parameter/type!='void'">
							<xsl:for-each select="parameters/parameter">
								<xsl:if test="name">
									<xsl:text>, </xsl:text>
									<xsl:choose>
										<xsl:when test="$isTemplate='true' and type='T'">
											<xsl:text>long</xsl:text>
										</xsl:when>
										<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
											<xsl:text>int</xsl:text>
										</xsl:when>
										<xsl:when test="type=root()/meta/namespace/typedef/@name and type!='String'">
											<xsl:text>long</xsl:text>
										</xsl:when>
										<xsl:when test="type='void'">
											<xsl:text>long</xsl:text>
										</xsl:when>
										<xsl:when test="not(contains($simpleTypes,type))">
											<xsl:text>long</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:call-template name="type">
												<xsl:with-param name="isParameter" select="'true'"/>
											</xsl:call-template>
										</xsl:otherwise>
									</xsl:choose>
									<xsl:text> </xsl:text>
									<xsl:value-of select="name"/>
								</xsl:if>
							</xsl:for-each>
						</xsl:if>
						<xsl:text>);</xsl:text>
						<xsl:text>&#10;</xsl:text>
					</xsl:when>
					<!-- ##################################################################### -->
					<!-- normal functions -->
					<xsl:otherwise>
						<xsl:choose>
							<!-- ToDo: more vecmath -->
							<xsl:when test="(type='Vector2' or type='Vector3' or type='Vector4') and $config/config/vecmath='true'">
								<xsl:text>float[]</xsl:text>
							</xsl:when>
							<xsl:when test="type=root()//class/@name or ((type=root()//typedef/@name and not(type=$typedefResultTree/mapping/@type) or substring-after(type,'.')=root()//typedef/@name) and type!='String') or type=root()//struct/@name or type='T.mapped_type' or type='T.key_type' or type='T.pointer' or type='T.const_pointer'">
								<xsl:text>long</xsl:text>
							</xsl:when>
							<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
								<xsl:text>int</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:choose>
									<xsl:when test="$isTemplate='true' and type='T'">
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type">
												<xsl:for-each select="root()/meta/namespace/class[@name=$currentClass]/inherits/baseClass">
													<xsl:if test="contains(current(),'&lt;')">
														<xsl:value-of select="current()"/>
													</xsl:if>
												</xsl:for-each>
											</xsl:with-param>
										</xsl:call-template>
									</xsl:when>
									<xsl:otherwise>
										<!-- _Constants -->
										<xsl:if test="type=root()/meta/namespace/enumeration/@name">
											<xsl:value-of select="$config/config/constants"/>
											<xsl:text>.</xsl:text>
										</xsl:if>
										<xsl:call-template name="type"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:text> _</xsl:text>
						<xsl:value-of select="name"/>
						<xsl:call-template name="nativeFuncSignature"/>
						<xsl:if test="@const='true'">
							<xsl:text>_const</xsl:text>
						</xsl:if>
						<xsl:text>(long pInstance</xsl:text>
						<xsl:for-each select="parameters/parameter">
							<xsl:if test="name or (type and type!='void')">
								<xsl:text>, </xsl:text>
								<xsl:choose>
									<xsl:when test="$isTemplate='true' and type='T'">
										<!--<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="template" select="root()/meta/namespace/class[@name=$currentClass]/inherits/baseClass"/>
										</xsl:call-template>-->
										<xsl:text>long</xsl:text>
									</xsl:when>
									<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
										<xsl:text>int</xsl:text>
									</xsl:when>
									<xsl:when test="type=root()/meta/namespace/typedef/@name and type!='String'">
										<xsl:text>long</xsl:text>
									</xsl:when>
									<xsl:when test="type='void'">
										<xsl:text>long</xsl:text>
									</xsl:when>
									<xsl:when test="not(contains($simpleTypes,type))">
										<xsl:text>long</xsl:text>
									</xsl:when>
									<xsl:otherwise>
										<xsl:call-template name="type">
											<xsl:with-param name="isParameter" select="'true'"/>
										</xsl:call-template>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:text> </xsl:text>
								<xsl:choose>
									<xsl:when test="name">
										<xsl:value-of select="name"/>
									</xsl:when>
									<xsl:otherwise>var</xsl:otherwise>
								</xsl:choose>
							</xsl:if>
						</xsl:for-each>
						<xsl:text>);&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<!-- constructor -->
			<xsl:when test="name=../@name and not($calledFrom='typedef')">
				<xsl:text>	private </xsl:text>
				<!-- no static in inner classes -->
				<xsl:if test="not(../../../class)">
					<xsl:text>static </xsl:text>
				</xsl:if>
				<xsl:text>native long _createInstance</xsl:text>
				<xsl:call-template name="nativeFuncSignature"/>
				<xsl:if test="@const='true'">
					<xsl:text>_const</xsl:text>
				</xsl:if>
				<xsl:text>(</xsl:text>
				<xsl:for-each select="parameters/parameter">
					<xsl:choose>
						<xsl:when test="not(contains($simpleTypes,type))">
							<xsl:text>long</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="type">
								<xsl:with-param name="isParameter" select="'true'"/>
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:text> </xsl:text>
					<xsl:value-of select="name"/>
					<xsl:if test="position()!=last()">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>);&#10;</xsl:text>
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	<!-- ################################ CTYPEDEF, for C++ wrapper ################################ -->
	<!-- cursor on meta/namespace/class/typedef -->
	<xsl:template name="cTypedef">
		<xsl:variable name="prefix" select="$config/config/prefix_cpp"/>
		<xsl:if test="../../class">
			<xsl:variable name="class" select="../@name"/>
			<xsl:variable name="typedef" select="@name"/>
			<!-- C++ Wrapper for the typedef -->
			<xsl:variable name="filename" select="concat($path,'/src/native/src/',$prefix,'_',$class,'_',$typedef,'.cpp')"/>
			<xsl:result-document href="{$filename}" format="textOutput">
				<xsl:if test="type!='' or typedef">
					<xsl:text>#include &lt;Precompiled.h&gt;&#10;</xsl:text>
					<xsl:text>#include &lt;</xsl:text>
					<xsl:value-of select="concat('org_ogre4j_',$class,'_',$typedef)"/>
					<xsl:text>.h&gt;&#10;</xsl:text>
					<xsl:choose>
						<!-- functions for std::map and std::pair and std::multimap -->
						<xsl:when test="@basetype='std::map' or @basetype='std::pair' or contains(@basetype,'std::hash_map') or (@basetype='std::multimap' and not(typedef))">
							<xsl:text>#include &lt;map&gt;&#10;&#10;</xsl:text>
							<xsl:call-template name="cMapFunctions"/>
						</xsl:when>
						<xsl:when test="@basetype='std::pair'">
							<xsl:text>#include &lt;pair&gt;&#10;&#10;</xsl:text>
							<xsl:call-template name="cMapFunctions"/>
						</xsl:when>
						<xsl:when test="@basetype='std::multimap'">
							<xsl:text>#include &lt;map&gt;&#10;&#10;</xsl:text>
							<xsl:call-template name="cMapFunctions"/>
						</xsl:when>
						<!-- functions for std::vector and std::list -->
						<xsl:when test="@basetype='std::vector'">
							<xsl:text>#include &lt;vector&gt;&#10;&#10;</xsl:text>
							<xsl:call-template name="cVectorFunctions"/>
						</xsl:when>
						<xsl:when test="@basetype='std::list'">
							<xsl:text>#include &lt;list&gt;&#10;&#10;</xsl:text>
							<xsl:call-template name="cVectorFunctions"/>
						</xsl:when>
						<!-- functions for std::set -->
						<xsl:when test="@basetype='std::set'">
							<xsl:text>#include &lt;set&gt;&#10;&#10;</xsl:text>
							<xsl:call-template name="cSetFunctions"/>
						</xsl:when>
						<!-- functions for std::deque -->
						<xsl:when test="@basetype='std::deque'">
							<xsl:text>#include &lt;deque&gt;&#10;&#10;</xsl:text>
							<xsl:call-template name="cDequeFunctions"/>
						</xsl:when>
						<!-- functions for std::hash_compare and std::multimap-->
						<xsl:when test="@basetype='stdext::hash_compare' or @basetype='std::multimap'">
						</xsl:when>
						<!-- functions for std::bitset (not implemented) -->
						<xsl:when test="@basetype='std::bitset'">
							<xsl:text>#include &lt;bitset&gt;&#10;&#10;</xsl:text>
						</xsl:when>
					</xsl:choose>
				</xsl:if>
			</xsl:result-document>
			<!-- C++ Header for the typedef -->
			<xsl:variable name="filename" select="concat($path,'/src/native/include/jni/',$prefix,'_',$class,'_',$typedef,'.h')"/>
			<xsl:result-document href="{$filename}" format="textOutput">
				<xsl:if test="type!='' or typedef">
					<xsl:text>#include &lt;jni.h&gt;&#10;&#10;</xsl:text>
					<xsl:text>#ifndef _Included_</xsl:text>
					<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
					<xsl:text>&#10;</xsl:text>
					<xsl:text>#define _Included_</xsl:text>
					<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
					<xsl:text>&#10;</xsl:text>
					<xsl:text>#ifdef __cplusplus&#10;</xsl:text>
					<xsl:text>extern "C" {&#10;</xsl:text>
					<xsl:text>#endif&#10;</xsl:text>
					<xsl:choose>
						<!-- functions for std::map and std::pair and std::multimap -->
						<xsl:when test="@basetype='std::map' or @basetype='std::pair' or contains(@basetype,'std::hash_map') or (@basetype='std::multimap' and not(typedef))">
							<xsl:call-template name="cMapFunctions">
								<xsl:with-param name="isHeader" select="'true'"/>
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@basetype='std::pair'">
							<xsl:call-template name="cMapFunctions">
								<xsl:with-param name="isHeader" select="'true'"/>
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@basetype='std::multimap'">
							<xsl:call-template name="cMapFunctions">
								<xsl:with-param name="isHeader" select="'true'"/>
							</xsl:call-template>
						</xsl:when>
						<!-- functions for std::vector and std::list -->
						<xsl:when test="@basetype='std::vector'">
							<xsl:call-template name="cVectorFunctions">
								<xsl:with-param name="isHeader" select="'true'"/>
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@basetype='std::list'">
							<xsl:call-template name="cVectorFunctions">
								<xsl:with-param name="isHeader" select="'true'"/>
							</xsl:call-template>
						</xsl:when>
						<!-- functions for std::set -->
						<xsl:when test="@basetype='std::set'">
							<xsl:call-template name="cSetFunctions">
								<xsl:with-param name="isHeader" select="'true'"/>
							</xsl:call-template>
						</xsl:when>
						<!-- functions for std::deque -->
						<xsl:when test="@basetype='std::deque'">
							<xsl:call-template name="cDequeFunctions">
								<xsl:with-param name="isHeader" select="'true'"/>
							</xsl:call-template>
						</xsl:when>
						<!-- functions for std::hash_compare and std::multimap-->
						<xsl:when test="@basetype='stdext::hash_compare' or @basetype='std::multimap'">
						</xsl:when>
						<!-- functions for std::bitset (not implemented) -->
						<xsl:when test="@basetype='std::bitset'">
							<xsl:text>#include &lt;bitset&gt;</xsl:text>
						</xsl:when>
					</xsl:choose>
					<xsl:text>&#10;</xsl:text>
					<xsl:text>#ifdef __cplusplus&#10;</xsl:text>
					<xsl:text>}&#10;</xsl:text>
					<xsl:text>#endif&#10;</xsl:text>
					<xsl:text>#endif&#10;</xsl:text>
				</xsl:if>
			</xsl:result-document>
		</xsl:if>
	</xsl:template>
	<!-- ###################################### STRUCT, for Java wrapper ################################### -->
	<xsl:template name="struct">
		<xsl:param name="isGlobal"/>
		<xsl:if test="$isGlobal='true'">
			<xsl:text>import </xsl:text>
			<xsl:value-of select="$config/config/wrapper_package"/>
			<xsl:text>.*;&#10;</xsl:text>
		</xsl:if>
		<xsl:text>public class </xsl:text>
		<xsl:value-of select="@name"/>
		<xsl:text> extends NativeObject {&#10;</xsl:text>
		<xsl:text>	protected </xsl:text>
		<xsl:value-of select="@name"/>
		<xsl:text>(InstancePointer pInstance) {&#10;</xsl:text>
		<xsl:text>		super(pInstance);&#10;</xsl:text>
		<xsl:text>	}&#10;</xsl:text>
		<xsl:text>	protected </xsl:text>
		<xsl:value-of select="@name"/>
		<xsl:text>(InstancePointer pInstance, boolean createdByLibrary) {&#10;</xsl:text>
		<xsl:text>		super(pInstance, createdByLibrary);&#10;</xsl:text>
		<xsl:text>	}&#10;</xsl:text>
		<xsl:text>	public void dispose() { }&#10;</xsl:text>
		<!-- struct private variables -->
		<xsl:for-each select="param">
			<xsl:if test="not(name=$ignore/ignorelist/ignore)">
				<xsl:text>	public </xsl:text>
				<!-- _Constants -->
				<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
					<xsl:value-of select="$config/config/constants"/>
					<xsl:text>.</xsl:text>
				</xsl:if>
				<xsl:call-template name="type">
					<xsl:with-param name="isStruct" select="'true'"/>
				</xsl:call-template>
				<xsl:text> </xsl:text>
				<xsl:value-of select="name"/>
				<xsl:text>;&#10;</xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:text>}&#10;</xsl:text>
	</xsl:template>
	<!-- ################################ TYPEDEF, for Java wrapper ############################# -->
	<!-- cursor on meta/namespace/class/typedef -->
	<xsl:template name="typedef">
		<xsl:variable name="reference" select="@type"/>
		<xsl:choose>
			<xsl:when test="type!='' or typedef">
				<xsl:text>	public class </xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text> extends NativeObject implements </xsl:text>
				<xsl:call-template name="STLMap"/>
				<xsl:text>&lt;</xsl:text>
				<xsl:for-each select="type">
					<xsl:choose>
						<xsl:when test="current()=$typedefResultTree/mapping/@type">
							<xsl:call-template name="templateTypeMap">
								<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
							</xsl:call-template>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="templateTypeMap">
								<xsl:with-param name="type" select="."/>
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:for-each select="typedef">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>&gt; {&#10;</xsl:text>
				<xsl:call-template name="stdFunctions">
					<xsl:with-param name="baseType" select="@basetype"/>
				</xsl:call-template>
				<xsl:text>}&#10;</xsl:text>
			</xsl:when>
			<!-- @baseclass=//typedef/@name -->
			<xsl:otherwise>
				<xsl:variable name="basetype" select="substring-before(@basetype,'&lt;')"/>
				<xsl:text>	public class </xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text> extends NativeObject</xsl:text>
				<xsl:if test="@basetype!=''">
					<xsl:text> implements </xsl:text>
					<xsl:value-of select="$basetype"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:choose>
						<!-- 2 template types, in current class -->
						<xsl:when test="../typedef[@name=$reference]/type[position()=2]">
							<xsl:for-each select="../typedef[@name=$reference]/type">
								<xsl:choose>
									<xsl:when test="current()=$typedefResultTree/mapping/@type">
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
										</xsl:call-template>
									</xsl:when>
									<xsl:otherwise>
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type" select="current()"/>
										</xsl:call-template>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:if test="position()!=last()">
									<xsl:text>,</xsl:text>
								</xsl:if>
							</xsl:for-each>
						</xsl:when>
						<!-- 2 template types, in global typedef -->
						<xsl:when test="root()//typedef[@name=$reference]/type[position()=2]">
							<xsl:for-each select="root()//typedef[@name=$reference]/type">
								<xsl:choose>
									<xsl:when test="current()=$typedefResultTree/mapping/@type">
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
										</xsl:call-template>
									</xsl:when>
									<xsl:otherwise>
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type" select="current()"/>
										</xsl:call-template>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:if test="position()!=last()">
									<xsl:text>,</xsl:text>
								</xsl:if>
							</xsl:for-each>
						</xsl:when>
						<!-- 1 template type, in current class -->
						<xsl:when test="../typedef[@name=$reference]">
							<xsl:for-each select="../typedef[@name=$reference]/type">
								<xsl:choose>
									<xsl:when test="current()=$typedefResultTree/mapping/@type">
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
										</xsl:call-template>
									</xsl:when>
									<xsl:otherwise>
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type" select="current()"/>
										</xsl:call-template>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:for-each>
						</xsl:when>
						<!-- 1 template type, in global typedef -->
						<xsl:when test="root()//typedef[@name=$reference]">
							<xsl:for-each select="root()//typedef[@name=$reference]/type">
								<xsl:choose>
									<xsl:when test="current()=$typedefResultTree/mapping/@type">
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=current()]"/>
										</xsl:call-template>
									</xsl:when>
									<xsl:otherwise>
										<xsl:call-template name="templateTypeMap">
											<xsl:with-param name="type" select="current()"/>
										</xsl:call-template>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:for-each>
						</xsl:when>
						<!-- no template type element -->
						<xsl:otherwise>
							<xsl:call-template name="templateTypeMap">
								<xsl:with-param name="type" select="@type"/>
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
					<!--
					<xsl:choose>
						<xsl:when test="contains(@basetype,'Ogre::')">
							<xsl:value-of select="substring-after(@basetype,'Ogre::')"/>
						</xsl:when>
						<xsl:when test="contains(@basetype,'Real') and not(@type=root()//typedef/@name)">
							<xsl:variable name="nativeType" select="concat('Native',@type)"/>
							<xsl:value-of select="replace(@basetype,@type,$nativeType)"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="@basetype"/>
						</xsl:otherwise>
					</xsl:choose>
					-->
					<xsl:text>&gt;</xsl:text>
					<xsl:text> {&#10;</xsl:text>
					<xsl:for-each select="root()//class[@name=$basetype]/function">
						<xsl:call-template name="function">
							<xsl:with-param name="isTemplate" select="'true'"/>
							<xsl:with-param name="typedefType" select="$reference"/>
						</xsl:call-template>
					</xsl:for-each>
				</xsl:if>
				<xsl:text>	protected </xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>(InstancePointer pInstance) {&#10;</xsl:text>
				<xsl:text>		super(pInstance);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- dispose() -->
				<xsl:text>	public void dispose() {&#10;</xsl:text>
				<xsl:choose>
					<xsl:when test="$config/config/dispose_exception/@use='true'">
						<xsl:text>	if (this.createdByLibrary)&#10;</xsl:text>
						<xsl:text>		throw new RuntimeException("can't dispose object created by the library");&#10;</xsl:text>
					</xsl:when>
					<xsl:when test="$config/config/dispose_exception/@use='false'">
						<xsl:text>	if (this.createdByLibrary) return;&#10;</xsl:text>
					</xsl:when>
				</xsl:choose>
				<xsl:text>		_dispose(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	private native void _dispose(long pInstance);&#10;</xsl:text>
				<xsl:for-each select="root()//class[@name=$basetype]/function">
					<xsl:call-template name="nativeFunction">
						<xsl:with-param name="isTemplate" select="'true'"/>
						<xsl:with-param name="calledFrom" select="'typedef'"/>
					</xsl:call-template>
				</xsl:for-each>
				<!--				<xsl:call-template name="iteratorFunctions"/>-->
				<xsl:text>	}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>&#10;</xsl:text>
	</xsl:template>
	<!-- ################################ STLMAP, for template name="typedef" ############################# -->
	<xsl:template name="STLMap">
		<xsl:choose>
			<xsl:when test="@basetype='std::map' or @basetype='std::multimap' or @basetype='std::pair' or contains(@basetype,'std::hash_map')">
				<xsl:text>Map</xsl:text>
			</xsl:when>
			<xsl:when test="@basetype='std::vector' or @basetype='std::list'">
				<xsl:text>List</xsl:text>
			</xsl:when>
			<xsl:when test="@basetype='std::set'">
				<xsl:text>Set</xsl:text>
			</xsl:when>
			<xsl:when test="@basetype='stdext::hash_compare'">
				<xsl:text>HashMap</xsl:text>
			</xsl:when>
			<xsl:when test="@basetype='std::deque'">
				<xsl:text>Queue</xsl:text>
			</xsl:when>
			<!-- not implemented yet
			<xsl:when test="@basetype='std::hashvector'">
				<xsl:text>HashVector</xsl:text>
			</xsl:when>
			-->
			<xsl:when test="count(type)=1">
				<xsl:text>NativeMap</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>HashMap</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- ############################### TYPEDEFMAPPING ################################# -->
	<xsl:template name="typedefMapping">
		<xsl:param name="name"/>
		<xsl:param name="originalName"/>
		<xsl:for-each select="../typedef[@name=$name]">
			<xsl:choose>
				<!-- recursion -->
				<xsl:when test="@basetype=../typedef/@name">
					<xsl:call-template name="typedefMapping">
						<xsl:with-param name="name" select="@basetype"/>
						<xsl:with-param name="originalName" select="$originalName"/>
					</xsl:call-template>
				</xsl:when>
				<!-- simple types -->
				<xsl:when test="@basetype='int'">
					<xsl:element name="mapping">
						<xsl:attribute name="type" select="$originalName"/>
						<xsl:value-of select="'int'"/>
						<!--<xsl:attribute name="value" select="'int'"/>-->
					</xsl:element>
				</xsl:when>
				<xsl:when test="@basetype='unsigned int'">
					<xsl:element name="mapping">
						<xsl:attribute name="type" select="$originalName"/>
						<xsl:value-of select="'long'"/>
					</xsl:element>
				</xsl:when>
				<xsl:when test="@basetype='unsigned __int64'">
					<xsl:element name="mapping">
						<xsl:attribute name="type" select="$originalName"/>
						<xsl:value-of select="'long'"/>
					</xsl:element>
				</xsl:when>
				<xsl:when test="@basetype='unsigned short'">
					<xsl:element name="mapping">
						<xsl:attribute name="type" select="$originalName"/>
						<xsl:value-of select="'int'"/>
					</xsl:element>
				</xsl:when>
				<xsl:when test="@basetype='unsigned char'">
					<xsl:element name="mapping">
						<xsl:attribute name="type" select="$originalName"/>
						<xsl:value-of select="'short'"/>
					</xsl:element>
				</xsl:when>
				<xsl:when test="@basetype='float'">
					<xsl:element name="mapping">
						<xsl:attribute name="type" select="$originalName"/>
						<xsl:value-of select="'float'"/>
					</xsl:element>
				</xsl:when>
				<xsl:when test="@basetype='unsigned long'">
					<xsl:element name="mapping">
						<xsl:attribute name="type" select="$originalName"/>
						<xsl:value-of select="'long'"/>
					</xsl:element>
				</xsl:when>
				<!-- enum -->
				<xsl:when test="starts-with(@basetype,'enum')">
					<xsl:element name="mapping">
						<xsl:attribute name="type" select="$originalName"/>
						<xsl:value-of select="substring-after(@basetype,'::')"/>
					</xsl:element>
				</xsl:when>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>
	<!-- ################################ CENUMERATION, for C++ wrapper ############################# -->
	<xsl:template name="cEnumeration">
		<xsl:param name="enumList"/>
		<xsl:param name="isHeader"/>
		<xsl:variable name="namespace">
			<xsl:if test="../../class">
				<xsl:value-of select="../../@name"/>
				<xsl:text>::</xsl:text>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="class" select="../@name"/>
		<xsl:variable name="enum" select="@name"/>
		<!-- toInt(Enum value) -->
		<xsl:choose>
			<xsl:when test="starts-with(@name,'@')">
				<xsl:text>int toInt(</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:value-of select="$class"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$config/config/enum_name"/>
				<xsl:text> value)</xsl:text>
				<xsl:choose>
					<xsl:when test="$isHeader='true'">
						<xsl:text>;&#10;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text> {&#10;</xsl:text>
						<xsl:text>	switch (value) {&#10;</xsl:text>
						<xsl:for-each select="$enumList/enum">
							<xsl:text>		case </xsl:text>
							<xsl:value-of select="$namespace"/>
							<xsl:value-of select="$class"/>
							<xsl:text>::</xsl:text>
							<xsl:value-of select="$config/config/enum_name"/>
							<xsl:text>::</xsl:text>
							<xsl:value-of select="@name"/>
							<xsl:text>:&#10;</xsl:text>
							<xsl:text>			return </xsl:text>
							<xsl:number level="single" count="enum" format="1"/>
							<xsl:text>;&#10;</xsl:text>
						</xsl:for-each>
						<xsl:text>		default:&#10;</xsl:text>
						<xsl:text>			break;&#10;</xsl:text>
						<xsl:text>	}&#10;</xsl:text>
						<xsl:text>}&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>int toInt(</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:value-of select="$class"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$enum"/>
				<xsl:text> value)</xsl:text>
				<xsl:choose>
					<xsl:when test="$isHeader='true'">
						<xsl:text>;&#10;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text> {&#10;</xsl:text>
						<xsl:text>	switch (value) {&#10;</xsl:text>
						<xsl:for-each select="enum">
							<xsl:text>		case </xsl:text>
							<xsl:value-of select="$namespace"/>
							<xsl:value-of select="$class"/>
							<xsl:text>::</xsl:text>
							<xsl:value-of select="$enum"/>
							<xsl:text>::</xsl:text>
							<xsl:value-of select="@name"/>
							<xsl:text>:&#10;</xsl:text>
							<xsl:text>			return </xsl:text>
							<xsl:number level="single" count="enum" format="1"/>
							<xsl:text>;&#10;</xsl:text>
						</xsl:for-each>
						<xsl:text>		default:&#10;</xsl:text>
						<xsl:text>			break;&#10;</xsl:text>
						<xsl:text>	}&#10;</xsl:text>
						<xsl:text>}&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
		<!-- toEnum(int value) -->
		<xsl:choose>
			<xsl:when test="starts-with(@name,'@')">
				<xsl:value-of select="$namespace"/>
				<xsl:value-of select="$class"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$config/config/enum_name"/>
				<xsl:text> to</xsl:text>
				<xsl:value-of select="$config/config/enum_name"/>
				<xsl:text>Enum(int value)</xsl:text>
				<xsl:choose>
					<xsl:when test="$isHeader='true'">
						<xsl:text>;&#10;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text> {&#10;</xsl:text>
						<xsl:text>	switch (value) {&#10;</xsl:text>
						<xsl:for-each select="$enumList/enum">
							<xsl:text>		case </xsl:text>
							<xsl:number level="single" count="enum" format="1"/>
							<xsl:text>:&#10;</xsl:text>
							<xsl:text>			return </xsl:text>
							<xsl:value-of select="$namespace"/>
							<xsl:value-of select="$class"/>
							<xsl:text>::</xsl:text>
							<xsl:value-of select="$config/config/enum_name"/>
							<xsl:text>::</xsl:text>
							<xsl:value-of select="@name"/>
							<xsl:text>;&#10;</xsl:text>
						</xsl:for-each>
						<xsl:text>		default:&#10;</xsl:text>
						<xsl:text>			break;&#10;</xsl:text>
						<xsl:text>	}&#10;</xsl:text>
						<xsl:text>}&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$namespace"/>
				<xsl:value-of select="$class"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$enum"/>
				<xsl:text> to</xsl:text>
				<xsl:value-of select="$enum"/>
				<xsl:text>Enum(int value)</xsl:text>
				<xsl:choose>
					<xsl:when test="$isHeader='true'">
						<xsl:text>;&#10;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text> {&#10;</xsl:text>
						<xsl:text>	switch (value) {&#10;</xsl:text>
						<xsl:for-each select="enum">
							<xsl:text>		case </xsl:text>
							<xsl:number level="single" count="enum" format="1"/>
							<xsl:text>:&#10;</xsl:text>
							<xsl:text>			return </xsl:text>
							<xsl:value-of select="$namespace"/>
							<xsl:value-of select="$class"/>
							<xsl:text>::</xsl:text>
							<xsl:value-of select="$enum"/>
							<xsl:text>::</xsl:text>
							<xsl:value-of select="@name"/>
							<xsl:text>;&#10;</xsl:text>
						</xsl:for-each>
						<xsl:text>		default:&#10;</xsl:text>
						<xsl:text>			break;&#10;</xsl:text>
						<xsl:text>	}&#10;</xsl:text>
						<xsl:text>}&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- ################################ ENUMERATION, for Java wrapper ############################# -->
	<xsl:template name="enumeration">
		<xsl:param name="enumList"/>
		<xsl:text>	public enum </xsl:text>
		<xsl:choose>
			<xsl:when test="starts-with(@name,'@')">
				<xsl:value-of select="$config/config/enum_name"/>
				<xsl:text> {&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:for-each select="$enumList/enum">
					<xsl:value-of select="@name"/>
					<xsl:text> (</xsl:text>
					<xsl:number level="single" count="enum" format="1"/>
					<xsl:text>)</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@name"/>
				<xsl:text> {&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:for-each select="enum">
					<xsl:value-of select="@name"/>
					<xsl:text> (</xsl:text>
					<xsl:number level="single" count="enum" format="1"/>
					<xsl:text>)</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>;&#10;&#10;</xsl:text>
		<xsl:text>		public int value;&#10;</xsl:text>
		<xsl:text>		</xsl:text>
		<xsl:choose>
			<xsl:when test="starts-with(@name,'@')">
				<xsl:value-of select="$config/config/enum_name"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@name"/>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>(int i) {&#10;</xsl:text>
		<xsl:text>			this.value = i;&#10;</xsl:text>
		<xsl:text>		}&#10;</xsl:text>
		<!-- enum mapping, switch case -->
		<xsl:text>		public static final </xsl:text>
		<xsl:choose>
			<xsl:when test="starts-with(@name,'@')">
				<xsl:value-of select="$config/config/enum_name"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@name"/>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text> toEnum(int retval) {&#10;</xsl:text>
		<xsl:text>			switch (retval) {&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="starts-with(@name,'@')">
				<xsl:for-each select="$enumList/enum">
					<xsl:text>				case </xsl:text>
					<xsl:number level="single" count="enum" format="1"/>
					<xsl:text>:&#10;</xsl:text>
					<xsl:text>					return </xsl:text>
					<xsl:value-of select="$config/config/enum_name"/>
					<xsl:text>.</xsl:text>
					<xsl:value-of select="@name"/>
					<xsl:text>;&#10;</xsl:text>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:for-each select="enum">
					<xsl:text>				case </xsl:text>
					<xsl:number level="single" count="enum" format="1"/>
					<xsl:text>:&#10;</xsl:text>
					<xsl:text>					return </xsl:text>
					<xsl:value-of select="../@name"/>
					<xsl:text>.</xsl:text>
					<xsl:value-of select="@name"/>
					<xsl:text>;&#10;</xsl:text>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>				default:&#10;</xsl:text>
		<xsl:text>					throw new RuntimeException("wrong number in jni call for an enum");&#10;</xsl:text>
		<xsl:text>			}&#10;</xsl:text>
		<xsl:text>		}&#10;</xsl:text>
		<xsl:text>	}&#10;</xsl:text>
	</xsl:template>
	<!-- ################################ COMMENT, for C++ ############################# -->
	<xsl:template name="comment">
		<xsl:text>&#10;</xsl:text>
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:		org_ogre4j_</xsl:text>
		<xsl:value-of select="../@name"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:		</xsl:text>
		<xsl:value-of select="name"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Signature:	(</xsl:text>
		<xsl:call-template name="signature">
			<xsl:with-param name="parameter" select="'param'"/>
		</xsl:call-template>
		<xsl:text>)</xsl:text>
		<xsl:call-template name="signature">
			<xsl:with-param name="parameter" select="'return'"/>
		</xsl:call-template>
		<xsl:text>;&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
	</xsl:template>
	<!-- ################################ SIGNATURE, for C++ ################################ -->
	<xsl:template name="signature">
		<xsl:param name="parameter"/>
		<xsl:choose>
			<xsl:when test="$parameter='param'">
				<xsl:if test="type">
					<xsl:text>J</xsl:text>
				</xsl:if>
				<xsl:for-each select="parameters/parameter">
					<xsl:if test="type and type!='void'">
						<xsl:call-template name="typemap"/>
					</xsl:if>
				</xsl:for-each>
			</xsl:when>
			<xsl:when test="$parameter='return'">
				<xsl:if test="not(type)">
					<xsl:text>V</xsl:text>
				</xsl:if>
				<xsl:call-template name="typemap"/>
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	<!-- ##################### TYPEMAP, for signatures ################### -->
	<xsl:template name="typemap">
		<xsl:choose>
			<xsl:when test="not(contains($simpleTypes,type))">
				<!--<xsl:text>J</xsl:text>-->
				<xsl:text>V</xsl:text>
			</xsl:when>
			<xsl:when test="type='String'">
				<xsl:text>Ljava/lang/String</xsl:text>
			</xsl:when>
			<xsl:when test="type='void'">
				<xsl:text>V</xsl:text>
			</xsl:when>
			<xsl:when test="type='boolean'">
				<xsl:text>Z</xsl:text>
			</xsl:when>
			<xsl:when test="type='byte'">
				<xsl:text>B</xsl:text>
			</xsl:when>
			<xsl:when test="type='char '">
				<xsl:text>C</xsl:text>
			</xsl:when>
			<xsl:when test="type='short'">
				<xsl:text>S</xsl:text>
			</xsl:when>
			<xsl:when test="type='int'">
				<xsl:text>I</xsl:text>
			</xsl:when>
			<xsl:when test="type='long'">
				<xsl:text>J</xsl:text>
			</xsl:when>
			<xsl:when test="type='float'">
				<xsl:text>F</xsl:text>
			</xsl:when>
			<xsl:when test="type='double'">
				<xsl:text>D</xsl:text>
			</xsl:when>
			<xsl:when test="type='int[]'">
				<xsl:text>[I</xsl:text>
			</xsl:when>
			<!--
			<xsl:otherwise>
				<xsl:text>l</xsl:text>
			</xsl:otherwise>
			-->
		</xsl:choose>
	</xsl:template>
	<!-- ##################### TEMPLATETYPEMAP, for java ################### -->
	<xsl:template name="templateTypeMap">
		<xsl:param name="type"/>
		<xsl:choose>
			<xsl:when test="contains($type,'&lt; ')">
				<xsl:variable name="string1" select="substring-after($type,'&lt; ')"/>
				<xsl:variable name="type" select="substring-before($string1,' &gt;')"/>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="$type"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$type='char'">
				<xsl:text>CharPointer</xsl:text>
			</xsl:when>
			<xsl:when test="$type='double'">
				<xsl:text>DoublePointer</xsl:text>
			</xsl:when>
			<xsl:when test="$type='int'">
				<xsl:text>IntegerPointer</xsl:text>
			</xsl:when>
			<xsl:when test="$type='Real' or $type='float'">
				<xsl:text>FloatPointer</xsl:text>
			</xsl:when>
			<xsl:when test="$type='long' or $type='uint'">
				<xsl:text>LongPointer</xsl:text>
			</xsl:when>
			<xsl:when test="$type='short' or $type='unsigned short' or $type='uint8'">
				<xsl:text>ShortPointer</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<!-- _Constants -->
				<xsl:if test="$type=root()/meta/namespace/enumeration/@name">
					<xsl:value-of select="$config/config/constants"/>
					<xsl:text>.</xsl:text>
				</xsl:if>
				<xsl:value-of select="$type"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- ################################ TYPE, for .java ################################ -->
	<!-- typemapping for functions and parameters -->
	<xsl:template name="type">
		<xsl:param name="isParameter"/>
		<xsl:param name="isStruct"/>
		<xsl:variable name="type" select="type"/>
		<xsl:choose>
			<!-- vecmath -->
			<xsl:when test="type='Matrix4' and $config/config/vecmath='true'">
				<xsl:text>Matrix4f</xsl:text>
			</xsl:when>
			<xsl:when test="type='Vector3' and $config/config/vecmath='true'">
				<xsl:text>Point3f</xsl:text>
			</xsl:when>
			<xsl:when test="type='Quaternion' and $config/config/vecmath='true'">
				<xsl:text>Quad4f</xsl:text>
			</xsl:when>
			<xsl:when test="type='Vector4' and $config/config/vecmath='true'">
				<xsl:text>Vector4f</xsl:text>
			</xsl:when>
			<!-- enumerations -->
			<xsl:when test="contains(type,'enum ')">
				<xsl:value-of select="substring-after(type,'enum ')"/>
			</xsl:when>
			<xsl:when test="type='void' and $isParameter='true'">
				<xsl:text>NativeObject</xsl:text>
			</xsl:when>
			<!-- typedef mapping -->
			<xsl:when test="type=$typedefResultTree/mapping/@type">
				<xsl:value-of select="$typedefResultTree/mapping[@type=$type]"/>
			</xsl:when>
			<!-- struct types -->
			<xsl:when test="starts-with(type,'std::list')">
				<xsl:value-of select="replace(type,'std::list','List')"/>
			</xsl:when>
			<!--
			<xsl:when test="starts-with(type,'std.map')">
				<xsl:value-of select="replace(type,'std.map','java.util.Map')"/>
			</xsl:when>
			-->
			<xsl:when test="type='void' and $isStruct='true'">
				<xsl:text>NativeObject</xsl:text>
			</xsl:when>
			<xsl:when test="contains(type,'Ogre::')">
				<xsl:value-of select="substring-after(type,'Ogre::')"/>
			</xsl:when>
			<!-- std -->
			<xsl:when test="starts-with(type,'std.pair') or starts-with(type,'std.map')">
				<xsl:choose>
					<xsl:when test="starts-with(type,'std.map')">
						<xsl:text>java.util.Map</xsl:text>
					</xsl:when>
					<xsl:when test="starts-with(type,'std.pair')">
						<xsl:text>java.util.Map.Entry</xsl:text>
					</xsl:when>
				</xsl:choose>
				<xsl:text>&lt;</xsl:text>
				<xsl:variable name="string1" select="substring-after(type,'&lt;')"/>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="normalize-space(substring-before($string1,','))"/>
				</xsl:call-template>
				<xsl:text>,</xsl:text>
				<xsl:variable name="string2" select="substring-before($string1,'&gt;')"/>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="normalize-space(substring-after($string2,','))"/>
				</xsl:call-template>
				<xsl:text>&gt;</xsl:text>
			</xsl:when>
			<xsl:when test="type=$config/config/std/@type">
				<xsl:value-of select="$config/config/std[@type=$type]"/>
			</xsl:when>
			<xsl:when test="contains(type,'&lt;')">
				<xsl:value-of select="substring-before(type,'&lt;')"/>
				<xsl:text>&lt;</xsl:text>
				<xsl:variable name="string1" select="substring-after(type,'&lt;')"/>
				<xsl:choose>
					<xsl:when test="contains($string1,'&lt;')">
						<xsl:value-of select="$string1"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="normalize-space(substring-before($string1,'&gt;'))"/>
						</xsl:call-template>
						<xsl:text>&gt;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<!-- normal types -->
			<xsl:otherwise>
				<xsl:value-of select="type"/>
				<xsl:if test="type/@array">
					<xsl:text>[]</xsl:text>
				</xsl:if>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- ############################### INTERFACE, for .java ############################### -->
	<!-- implemented functions of the used interface must be implemented in the actual class -->
	<!-- cursor on meta/namespace/class/inherits/baseClass -->
	<xsl:template name="interface">
		<xsl:param name="name"/>
		<xsl:for-each select="root()//class[@name=$name]">
			<!-- cursor on meta/namespace/class -->
			<xsl:variable name="filename" select="concat($path,'/src/java/org/ogre4j/I',@name,'.java')"/>
			<xsl:result-document href="{$filename}" format="textOutput">
				<xsl:text>package org.ogre4j;&#10;</xsl:text>
				<xsl:text>&#10;</xsl:text>
				<xsl:call-template name="imports"/>
				<xsl:text>public interface I</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text> {&#10;</xsl:text>
				<!-- function declaration, the same like in Java Proxy -->
				<xsl:for-each select="function">
					<xsl:if test="type">
						<xsl:choose>
							<!-- function type not(simpleTypes), developer must pass object as parameter because function is passed by value -->
							<xsl:when test="(type and type!='void' and not(contains($simpleTypes,type)) and not(type=$typedefResultTree/mapping/@type) and @passedBy='value') or ( type=root()//typedef/@name and not(type=$typedefResultTree/mapping/@type) and type!='String')">
								<!-- function declaration -->
								<xsl:text>	public void </xsl:text>
								<xsl:value-of select="name"/>
								<xsl:if test="@const='true'">
									<xsl:text>_const</xsl:text>
								</xsl:if>
								<xsl:text>(</xsl:text>
								<xsl:if test="type!='void'">
									<!-- _Constants -->
									<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
										<xsl:value-of select="$config/config/constants"/>
										<xsl:text>.</xsl:text>
									</xsl:if>
									<xsl:call-template name="type"/>
									<xsl:text> new</xsl:text>
									<xsl:choose>
										<xsl:when test="starts-with(type,'std.map')">
											<xsl:text>Map</xsl:text>
										</xsl:when>
										<xsl:when test="starts-with(type,'std.pair')">
											<xsl:text>Map.Entry</xsl:text>
										</xsl:when>
										<xsl:when test="contains(type,'&lt;')">
											<xsl:value-of select="substring-before(type,'&lt;')"/>
										</xsl:when>
										<xsl:when test="contains(type,'.')">
											<xsl:value-of select="substring-after(type,'.')"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="type"/>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:if>
								<xsl:if test="parameters/parameter/type!='void'">
									<xsl:text>, </xsl:text>
									<xsl:for-each select="parameters/parameter">
										<!-- _Constants -->
										<xsl:if test="type=root()/meta/namespace/enumeration/@name or substring-after(type,'.')=root()/meta/namespace/enumeration/@name">
											<xsl:value-of select="$config/config/constants"/>
											<xsl:text>.</xsl:text>
										</xsl:if>
										<xsl:call-template name="type">
											<xsl:with-param name="isParameter" select="'true'"/>
										</xsl:call-template>
										<xsl:text> </xsl:text>
										<xsl:value-of select="name"/>
										<xsl:if test="position()!=last()">
											<xsl:text>, </xsl:text>
										</xsl:if>
									</xsl:for-each>
								</xsl:if>
							</xsl:when>
							<!-- normal functions -->
							<xsl:otherwise>
								<xsl:text>	public </xsl:text>
								<!-- _Constants -->
								<xsl:if test="type=root()/meta/namespace/enumeration/@name">
									<xsl:value-of select="$config/config/constants"/>
									<xsl:text>.</xsl:text>
								</xsl:if>
								<xsl:call-template name="type"/>
								<xsl:text> </xsl:text>
								<xsl:value-of select="name"/>
								<xsl:if test="@const='true'">
									<xsl:text>_const</xsl:text>
								</xsl:if>
								<xsl:text>(</xsl:text>
								<xsl:if test="parameters/parameter/type!='void'">
									<xsl:for-each select="parameters/parameter">
										<!-- _Constants -->
										<xsl:if test="type=root()/meta/namespace/enumeration/@name">
											<xsl:value-of select="$config/config/constants"/>
											<xsl:text>.</xsl:text>
										</xsl:if>
										<xsl:call-template name="type">
											<xsl:with-param name="isParameter" select="'true'"/>
										</xsl:call-template>
										<xsl:text> </xsl:text>
										<xsl:value-of select="name"/>
										<xsl:if test="position()!=last()">
											<xsl:text>, </xsl:text>
										</xsl:if>
									</xsl:for-each>
								</xsl:if>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:text>);&#10;</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>}</xsl:text>
			</xsl:result-document>
		</xsl:for-each>
	</xsl:template>
	<!-- ########################## IMPORTS, for .java ########################### -->
	<!-- cursor on meta/namespace/class -->
	<xsl:template name="imports">
		<xsl:param name="calledFrom"/>
		<!--	<xsl:if test="function/parameters/parameter/type!=root()//class/@name and not(contains($simpleTypes,function/parameters/parameter/type))">
		<xsl:text>import org.ogre4j._Constants.*;&#10;</xsl:text>
		</xsl:if>-->
		<xsl:text>import </xsl:text>
		<xsl:value-of select="$config/config/wrapper_package"/>
		<xsl:text>.*;&#10;</xsl:text>
		<xsl:for-each select="struct/param/type">
			<xsl:if test="starts-with(.,'std::list')">
				<xsl:text>import java.util.List;&#10;</xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:if test="typedef/@basetype='HashMap'">
			<xsl:text>import java.util.HashMap;&#10;</xsl:text>
		</xsl:if>
		<xsl:if test="typedef/@basetype='std::deque'">
			<xsl:text>import java.util.Queue;&#10;</xsl:text>
		</xsl:if>
		<xsl:if test="typedef or $calledFrom='globalTypedef'">
			<xsl:choose>
				<xsl:when test="(typedef/@basetype='std::vector' or typedef/@basetype='std::list') or (@basetype='std::vector' or @basetype='std::list')">
					<xsl:text>import java.util.Collection;&#10;</xsl:text>
					<xsl:text>import java.util.List;&#10;</xsl:text>
					<xsl:text>import java.util.ListIterator;&#10;</xsl:text>
					<xsl:text>import java.util.Iterator;&#10;</xsl:text>
					<xsl:if test="typedef/type!='' or type!=''">
						<xsl:text>import java.util.Map;&#10;</xsl:text>
						<xsl:text>import java.util.Set;&#10;</xsl:text>
					</xsl:if>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="typedef/type!='' or type!=''">
						<xsl:text>import java.util.Collection;&#10;</xsl:text>
						<xsl:text>import java.util.Map;&#10;</xsl:text>
						<xsl:text>import java.util.Set;&#10;</xsl:text>
					</xsl:if>
					<xsl:text>import java.util.Iterator;&#10;</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
		<!--
		<xsl:if test="function/type='Matrix4' or function/parameters/parameter/type='Matrix4'">
			<xsl:text>import javax.vecmath.Matrix4f;&#10;</xsl:text>
		</xsl:if>
		<xsl:if test="function/type='Vector3' or function/parameters/parameter/type='Vector3'">
			<xsl:text>import javax.vecmath.Point3f;&#10;</xsl:text>
		</xsl:if>
		<xsl:if test="function/type='Quaternion' or function/parameters/parameter/type='Quaternion'">
			<xsl:text>import javax.vecmath.Quat4f;&#10;</xsl:text>
		</xsl:if>
		<xsl:if test="function/type='Vector4' or function/parameters/parameter/type='Vector4'">
			<xsl:text>import javax.vecmath.Vector4f;&#10;</xsl:text>
		</xsl:if>
		-->
		<xsl:text>&#10;</xsl:text>
		<xsl:text>&#10;</xsl:text>
	</xsl:template>
	<!-- ################################ CDECLARATION ################################ -->
	<xsl:template name="cDeclaration">
		<xsl:text>JNIEXPORT </xsl:text>
		<xsl:choose>
			<xsl:when test="type='void'">
				<xsl:text>void</xsl:text>
			</xsl:when>
			<xsl:when test="type=root()//enumeration/@name or substring-after(type,'.')=root()//enumeration/@name">
				<xsl:text>jint</xsl:text>
			</xsl:when>
			<xsl:when test="type='String'">
				<xsl:text>jstring</xsl:text>
			</xsl:when>
			<xsl:when test="type and contains($simpleTypes,type)">
				<xsl:text>j</xsl:text>
				<xsl:value-of select="type"/>
			</xsl:when>
			<xsl:when test="type and not(contains($simpleTypes,type))">
				<xsl:text>jlong</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>void</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text> JNICALL Java_org_ogre4j_</xsl:text>
		<xsl:value-of select="../@name"/>
		<xsl:text>__1</xsl:text>
		<xsl:choose>
			<!-- functions -->
			<xsl:when test="type">
				<xsl:value-of select="name"/>
			</xsl:when>
			<!-- constructor -->
			<xsl:otherwise>
				<xsl:text>createInstance</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!--<xsl:if test="parameters and parameters/parameter/type!='void'">-->
		<!-- if function name occurs more than once, append signature -->
		<!--		<xsl:if test="name=preceding-sibling::node()/name or name=following-sibling::node()/name">-->
		<!-- call template signature -->
		<xsl:text>_</xsl:text>
		<xsl:call-template name="nativeFuncSignature">
			<xsl:with-param name="calledFrom" select="'cHeader'"/>
		</xsl:call-template>
		<xsl:if test="@const='true'">
			<xsl:text>_1const</xsl:text>
		</xsl:if>
		<!--		</xsl:if>-->
	</xsl:template>
	<!-- ################################ JHEADER ################################ -->
	<xsl:template name="jHeader">
		<xsl:param name="name"/>
		<xsl:text>/* This source file is part of ogre4j&#10;</xsl:text>
		<xsl:text> *     (The JNI bindings for OGRE)&#10;</xsl:text>
		<xsl:text> * For the latest info, see http://www.ogre4j.org/&#10;</xsl:text>
		<xsl:text> * &#10;</xsl:text>
		<xsl:text> * Copyright (c) 2005 netAllied GmbH, Tettnang&#10;</xsl:text>
		<xsl:text> * Also see acknowledgements in Readme.html&#10;</xsl:text>
		<xsl:text> * &#10;</xsl:text>
		<xsl:text> * This program is free software; you can redistribute it and/or modify it under&#10;</xsl:text>
		<xsl:text> * the terms of the GNU Lesser General Public License as published by the Free Software&#10;</xsl:text>
		<xsl:text> * Foundation; either version 2 of the License, or (at your option) any later&#10;</xsl:text>
		<xsl:text> * version.&#10;</xsl:text>
		<xsl:text> * &#10;</xsl:text>
		<xsl:text> * This program is distributed in the hope that it will be useful, but WITHOUT&#10;</xsl:text>
		<xsl:text> * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS&#10;</xsl:text>
		<xsl:text> * FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.&#10;</xsl:text>
		<xsl:text> * &#10;</xsl:text>
		<xsl:text> * You should have received a copy of the GNU Lesser General Public License along with&#10;</xsl:text>
		<xsl:text> * this program; if not, write to the Free Software Foundation, Inc., 59 Temple&#10;</xsl:text>
		<xsl:text> * Place - Suite 330, Boston, MA 02111-1307, USA, or go to&#10;</xsl:text>
		<xsl:text> * http://www.gnu.org/copyleft/lesser.txt.&#10;</xsl:text>
		<xsl:text> * &#10;</xsl:text>
		<xsl:text> *&#10;</xsl:text>
		<xsl:text> * </xsl:text>
		<xsl:value-of select="$name"/>
		<xsl:text>.java&#10;</xsl:text>
		<xsl:text> * &#10;</xsl:text>
		<xsl:text> * automatically generated file&#10;</xsl:text>
		<xsl:text> * ----------------------&#10;</xsl:text>
		<xsl:text> * Date </xsl:text>
		<xsl:value-of select="current-date()"/>
		<xsl:text> &#10;</xsl:text>
		<xsl:text> * ----------------------&#10;</xsl:text>
		<!--		<xsl:text> * Author: author   Kai Klesatschke &lt;yavin@ogre4j.org&gt;&#10;</xsl:text> -->
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>&#10;</xsl:text>
	</xsl:template>
	<!-- ################################ CHEADER ################################ -->
	<xsl:template name="cHeader">
		<xsl:param name="fullName"/>
		<xsl:text>///////////////////////////////////////////////////////////////////////////////&#10;</xsl:text>
		<xsl:text>/// This source file is part of ogre4j&#10;</xsl:text>
		<xsl:text>///     (The JNI bindings for OGRE)&#10;</xsl:text>
		<xsl:text>/// For the latest info, see http://www.ogre4j.org/&#10;</xsl:text>
		<xsl:text>/// &#10;</xsl:text>
		<xsl:text>/// Copyright (c) 2005 netAllied GmbH, Tettnang&#10;</xsl:text>
		<xsl:text>/// Also see acknowledgements in Readme.html&#10;</xsl:text>
		<xsl:text>/// &#10;</xsl:text>
		<xsl:text>/// This program is free software; you can redistribute it and/or modify it under&#10;</xsl:text>
		<xsl:text>/// the terms of the GNU Lesser General Public License as published by the Free Software&#10;</xsl:text>
		<xsl:text>/// Foundation; either version 2 of the License, or (at your option) any later&#10;</xsl:text>
		<xsl:text>/// version.&#10;</xsl:text>
		<xsl:text>/// &#10;</xsl:text>
		<xsl:text>/// This program is distributed in the hope that it will be useful, but WITHOUT&#10;</xsl:text>
		<xsl:text>/// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS&#10;</xsl:text>
		<xsl:text>/// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.&#10;</xsl:text>
		<xsl:text>/// &#10;</xsl:text>
		<xsl:text>/// You should have received a copy of the GNU Lesser General Public License along with&#10;</xsl:text>
		<xsl:text>/// this program; if not, write to the Free Software Foundation, Inc., 59 Temple&#10;</xsl:text>
		<xsl:text>/// Place - Suite 330, Boston, MA 02111-1307, USA, or go to&#10;</xsl:text>
		<xsl:text>/// http://www.gnu.org/copyleft/lesser.txt.&#10;</xsl:text>
		<xsl:text>///&#10;</xsl:text>
		<xsl:text>/// Machine generated file&#10;</xsl:text>
		<xsl:text>/// @file </xsl:text>
		<xsl:value-of select="../../@name"/>
		<xsl:text>.cpp&#10;</xsl:text>
		<xsl:text>/// Implementation of the JNI bindings for the usage of the &#10;</xsl:text>
		<xsl:text>/// </xsl:text>
		<xsl:value-of select="$fullName"/>
		<xsl:text> class&#10;</xsl:text>
		<xsl:text>///&#10;</xsl:text>
		<!--		<xsl:text>/// @author   Kai Klesatschke &lt;yavin@ogre4j.org&gt;&#10;</xsl:text> -->
		<xsl:text>/// Date     </xsl:text>
		<xsl:value-of select="current-date()"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text>///////////////////////////////////////////////////////////////////////////////&#10;</xsl:text>
		<xsl:text>&#10;</xsl:text>
	</xsl:template>
	<!-- ################################ CMAPFUNCTIONS ################################ -->
	<!-- cursor on meta/namespace/class/typedef -->
	<xsl:template name="cMapFunctions">
		<xsl:param name="isHeader"/>
		<xsl:variable name="prefix" select="$config/config/prefix_cpp"/>
		<xsl:variable name="namespace" select="../../@name"/>
		<xsl:variable name="class" select="../@name"/>
		<xsl:variable name="typedef" select="@name"/>
		<!-- dispose() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _dispose&#10;</xsl:text>
		<xsl:text> * Signature: (J)V&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT void JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1dispose&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	delete map;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- size() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _size&#10;</xsl:text>
		<xsl:text> * Signature: (J)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jint JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1size&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	return map->size();&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- isEmpty() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _isEmpty&#10;</xsl:text>
		<xsl:text> * Signature: (J)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1isEmpty&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	if(map->empty())&#10;</xsl:text>
				<xsl:text>		return 1;&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- containsKey() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _containsKey&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1containsKey&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong key)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	int * pInt = reinterpret_cast&lt;int*&gt;(key);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>::iterator it = map->find(*pInt);&#10;</xsl:text>
				<xsl:text>	if(it == map->end())&#10;</xsl:text>
				<xsl:text>		return 0;&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- containsValue() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _containsValue&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1containsValue&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong value)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	int * pInt = reinterpret_cast&lt;int*&gt;(value);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>::iterator it = map->find(*pInt);&#10;</xsl:text>
				<xsl:text>	if(it == map->end())&#10;</xsl:text>
				<xsl:text>		return 0;&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- get() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _get&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)J&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jlong JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1get&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong key)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	int * pInt = reinterpret_cast&lt;int*&gt;(key);&#10;</xsl:text>
				<xsl:text>	return reinterpret_cast&lt;jlong&gt;(&amp;map[*pInt]);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- put() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _put&#10;</xsl:text>
		<xsl:text> * Signature: (JJJ)J&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jlong JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1put&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong key, jlong value)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	int * pInt = reinterpret_cast&lt;int*&gt;(key);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type[position()=2]"/>
				<xsl:text> * p</xsl:text>
				<xsl:value-of select="type[position()=2]"/>
				<xsl:text> = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type[position()=2]"/>
				<xsl:text>*&gt;(value);&#10;</xsl:text>
				<xsl:text>	map->insert(std::pair&lt;</xsl:text>
				<xsl:if test="not(contains($simpleTypes,type[position()=1])) or type[position()=1]='String'">
					<xsl:value-of select="$namespace"/>
					<xsl:text>::</xsl:text>
				</xsl:if>
				<xsl:value-of select="type[position()=1]"/>
				<xsl:text>,</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type[position()=2]"/>
				<xsl:text>&gt;(*pInt, *p</xsl:text>
				<xsl:value-of select="type[position()=2]"/>
				<xsl:text>));&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- remove() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _remove&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)J&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jlong JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1remove&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong key)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	int * pInt = reinterpret_cast&lt;int*&gt;(key);&#10;</xsl:text>
				<xsl:text>	map->erase(key);&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- putAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _putAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)J&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jlong JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1putAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text> (JNIEnv *env, jobject that, jlong pInstance, jlong t)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * otherMap = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>*&gt;(t);&#10;</xsl:text>
				<xsl:text>	map->insert(otherMap->begin(), otherMap->end());&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- clear() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _clear&#10;</xsl:text>
		<xsl:text> * Signature: (J)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT void JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1clear&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text> (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * map = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	map->erase(map->begin(), map->end());&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- ################################ CVECTORFUNCTIONS ################################ -->
	<!-- cursor on meta/namespace/class/typedef -->
	<xsl:template name="cVectorFunctions">
		<xsl:param name="isHeader"/>
		<xsl:variable name="prefix" select="$config/config/prefix_cpp"/>
		<xsl:variable name="namespace" select="../../@name"/>
		<xsl:variable name="class" select="../@name"/>
		<xsl:variable name="typedef" select="@name"/>
		<!-- dispose() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _dispose&#10;</xsl:text>
		<xsl:text> * Signature: (J)V&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT void JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1dispose&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	delete vector;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- size() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _size&#10;</xsl:text>
		<xsl:text> * Signature: (J)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jint JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1size&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	return vector->size();&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- isEmpty() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _isEmpty&#10;</xsl:text>
		<xsl:text> * Signature: (J)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1isEmpty&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	if(vector->empty())&#10;</xsl:text>
				<xsl:text>		return 1;&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- contains() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _contains&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1contains&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> * pO = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>*&gt;(o);&#10;</xsl:text>
				<xsl:text>	for(size_t i=0, size=vector->size(); i&lt;size; i++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> &amp;frame = vector->at(i);&#10;</xsl:text>
				<xsl:text>		if(&amp;frame==&amp;(*pO))&#10;</xsl:text>
				<xsl:text>			return 1;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- add() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _add&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1add__JJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> * p</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>*&gt;(o);&#10;</xsl:text>
				<xsl:text>	vector->push_back(*p</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>);&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- remove() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _remove&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1remove__JJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;::iterator it;&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> * </xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>*&gt;(o);</xsl:text>
				<xsl:text>	for (it=vector->begin(); it!=vector->end(); it++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> frame = *it;&#10;</xsl:text>
				<xsl:text>		if (&amp;frame==&amp;(*</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>))&#10;</xsl:text>
				<xsl:text>			break;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	if (it==vector->end())&#10;</xsl:text>
				<xsl:text>		return 0;&#10;</xsl:text>
				<xsl:text>	vector->erase(it);&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- containsAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _containsAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1containsAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt; * otherVector = reinterpret_cast&lt;std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;*&gt;(o);&#10;</xsl:text>
				<xsl:text>	for(size_t i=0, size=otherVector->size(); i&lt;size; i++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		jlong frame = reinterpret_cast&lt;jlong&gt;(&amp;otherVector->at(i));&#10;</xsl:text>
				<xsl:text>		if(Java_Ogre4J_</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="$typedef"/>
				<xsl:text>__1contains(env, that, pInstance, frame)==0)&#10;</xsl:text>
				<xsl:text>			return 0;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- addAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _addAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1addAll__JJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt; * otherVector = reinterpret_cast&lt;std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;*&gt;(o);&#10;</xsl:text>
				<xsl:text>	vector->insert(vector->begin(), otherVector->begin(), otherVector->end());&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- addAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _addAll&#10;</xsl:text>
		<xsl:text> * Signature: (JIJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1addAll__JIJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jint, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jint index, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt; * otherVector = reinterpret_cast&lt;std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;*&gt;(o);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;::iterator it = vector->begin();&#10;</xsl:text>
				<xsl:text>	it += index;&#10;</xsl:text>
				<xsl:text>	vector->insert(it, otherVector->begin(), otherVector->end());&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- removeAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _removeAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1removeAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt; * otherVector = reinterpret_cast&lt;std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;*&gt;(o);&#10;</xsl:text>
				<xsl:text>	for(size_t i=0, size=otherVector->size(); i&lt;size; i++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		jlong frame = reinterpret_cast&lt;jlong&gt;(&amp;otherVector->at(i));&#10;</xsl:text>
				<xsl:text>		Java_Ogre4J_</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="$typedef"/>
				<xsl:text>__1remove__JJ(env, that, pInstance, frame);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- retainAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _retainAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1retainAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt; * otherVector = reinterpret_cast&lt;std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;*&gt;(o);&#10;</xsl:text>
				<xsl:text>	for(size_t i=0, size=vector->size(); i&lt;size; i++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		jlong frame = reinterpret_cast&lt;jlong&gt;(&amp;vector->at(i));&#10;</xsl:text>
				<xsl:text>		if(Java_Ogre4J_</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="$typedef"/>
				<xsl:text>__1contains(env, that, o, frame)==0)&#10;</xsl:text>
				<xsl:text>			Java_Ogre4J_</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="$typedef"/>
				<xsl:text>__1remove__JJ(env, that, pInstance, frame);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- clear() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _clear&#10;</xsl:text>
		<xsl:text> * Signature: (J)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT void JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1clear&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text> (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	vector->erase(vector->begin());&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- get() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _get&#10;</xsl:text>
		<xsl:text> * Signature: (JI)J&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jlong JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1get&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jint);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jint index)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> &amp;frame = vector->at(index);&#10;</xsl:text>
				<xsl:text>	return reinterpret_cast&lt;jlong&gt;(&amp;frame);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- set() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _set&#10;</xsl:text>
		<xsl:text> * Signature: (JIJ)J&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jlong JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1set&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jint, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jint index, jlong element)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> * pElement = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>*&gt;(element);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;::iterator it = vector->begin();&#10;</xsl:text>
				<xsl:text>	it += index;&#10;</xsl:text>
				<xsl:text>	*vector->insert(it, *pElement);&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- add() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _add&#10;</xsl:text>
		<xsl:text> * Signature: (JIJ)J&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jlong JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1add__JIJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jint, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jint index, jlong element)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt; * pElement = reinterpret_cast&lt;std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;*&gt;(element);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;::iterator it = vector->begin();&#10;</xsl:text>
				<xsl:text>	it += index;&#10;</xsl:text>
				<xsl:text>	vector->insert(it, pElement->begin(), pElement->end());&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- remove() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _remove&#10;</xsl:text>
		<xsl:text> * Signature: (JI)J&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jlong JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1remove__JI&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jint);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jint index)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;::iterator it = vector->begin();&#10;</xsl:text>
				<xsl:text>	it += index;&#10;</xsl:text>
				<xsl:text>	vector->erase(it);&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- indexOf() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _indexOf&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jint JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1indexOf&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt; * otherVector = reinterpret_cast&lt;std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;*&gt;(o);&#10;</xsl:text>
				<xsl:text>	for(size_t i=0, size=vector->size(); i&lt;size; i++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		if (Java_Ogre4J_</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="$typedef"/>
				<xsl:text>__1contains(env, that, pInstance, o)==1)&#10;</xsl:text>
				<xsl:text>			return i;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return -1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- lastIndexOf() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _lastIndexOf&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jint JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1lastIndexOf&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * vector = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt; * otherVector = reinterpret_cast&lt;std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;*&gt;(o);&#10;</xsl:text>
				<xsl:text>	for(size_t i=0, size=vector->size(); i&lt;size; i++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		if (Java_Ogre4J_AnimationTrack_KeyFrameList__1contains(env, that, pInstance, o)==1)&#10;</xsl:text>
				<xsl:text>			return i;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return -1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- ################################ CSETFUNCTIONS ################################ -->
	<!-- cursor on meta/namespace/class/typedef -->
	<xsl:template name="cSetFunctions">
		<xsl:param name="isHeader"/>
		<xsl:variable name="prefix" select="$config/config/prefix_cpp"/>
		<xsl:variable name="namespace" select="../../@name"/>
		<xsl:variable name="class" select="../@name"/>
		<xsl:variable name="typedef" select="@name"/>
		<!-- dispose() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _dispose&#10;</xsl:text>
		<xsl:text> * Signature: (J)V&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT void JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1dispose&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	delete set;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- size() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _size&#10;</xsl:text>
		<xsl:text> * Signature: (J)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jint JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1size&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	return set->size();&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- isEmpty() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _isEmpty&#10;</xsl:text>
		<xsl:text> * Signature: (J)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1isEmpty&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	if(set->empty())&#10;</xsl:text>
				<xsl:text>		return 1;&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- contains() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _contains&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1contains&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> * pO = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>*&gt;(o);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>::iterator it = set->find(*pO);&#10;</xsl:text>
				<xsl:text>	if(it == set->end())&#10;</xsl:text>
				<xsl:text>		return 0;&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- add() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _add&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1add__JJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> * pO = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>*&gt;(o);&#10;</xsl:text>
				<xsl:text>	set->insert(pO);&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- remove() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _remove&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1remove&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>::iterator it;&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> * pO = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>*&gt;(o);&#10;</xsl:text>
				<xsl:text>	for (it=set->begin(); it!=set->end(); it++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> entity = *it;&#10;</xsl:text>
				<xsl:text>		if (&amp;entity==&amp;(*pO))&#10;</xsl:text>
				<xsl:text>			break;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	if (it==set->end())&#10;</xsl:text>
				<xsl:text>		return 0;&#10;</xsl:text>
				<xsl:text>	set->erase(it);&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- containsAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _containsAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1containsAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * otherSet = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>*&gt;(o);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>::iterator it;&#10;</xsl:text>
				<xsl:text>	for(it = otherSet->begin(); it != otherSet->end(); it++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> * pEntity = *it;&#10;</xsl:text>
				<xsl:text>		jlong ref = reinterpret_cast&lt;jlong&gt;(pEntity);&#10;</xsl:text>
				<xsl:text>		if(Java_Ogre4J_</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="$typedef"/>
				<xsl:text>__1contains(env, that, pInstance, ref)==0)&#10;</xsl:text>
				<xsl:text>			return 0;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- addAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _addAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1addAll__JJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * pO = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(o);&#10;</xsl:text>
				<xsl:text>	set->insert(pO->begin(), pO->end());&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- addAll() 
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _addAll&#10;</xsl:text>
		<xsl:text> * Signature: (JIJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1addAll__JIJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jint, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jint index, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	std::set * set = reinterpret_cast<![CDATA[<std::set*>]]>(pInstance);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose> -->
		<!-- removeAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _removeAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1removeAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>::iterator it;&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> * pO = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>*&gt;(o);&#10;</xsl:text>
				<xsl:text>	for (it=set->begin(); it!=set->end(); it++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> entity = *it;&#10;</xsl:text>
				<xsl:text>		if (&amp;entity==&amp;(*pO))&#10;</xsl:text>
				<xsl:text>			set->erase(it);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- retainAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _retainAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1retainAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>::iterator it;&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> * pO = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> *&gt;(o);&#10;</xsl:text>
				<xsl:text>	for (it=set->begin(); it!=set->end(); it++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text> entity = *it;&#10;</xsl:text>
				<xsl:text>		if (&amp;entity!=&amp;(*pO))&#10;</xsl:text>
				<xsl:text>			set->erase(it);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- clear() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _clear&#10;</xsl:text>
		<xsl:text> * Signature: (J)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT void JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1clear&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text> (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * set = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	set->erase(set->begin());&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- ################################ CDEQUEFUNCTIONS ################################ -->
	<!-- cursor on meta/namespace/class/typedef -->
	<xsl:template name="cDequeFunctions">
		<xsl:param name="isHeader"/>
		<xsl:variable name="prefix" select="$config/config/prefix_cpp"/>
		<xsl:variable name="namespace" select="../../@name"/>
		<xsl:variable name="class" select="../@name"/>
		<xsl:variable name="typedef" select="@name"/>
		<!-- dispose() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _dispose&#10;</xsl:text>
		<xsl:text> * Signature: (J)V&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT void JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1dispose&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	delete deque;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- offer() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _offer&#10;</xsl:text>
		<xsl:text> * Signature: (JLorg/ogre4j/String;)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1offer&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jstring);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jstring o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:choose>
					<xsl:when test="type='String'">
						<xsl:text>std::string</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="stdType"/>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text> &amp; pO = reinterpret_cast&lt;</xsl:text>
				<xsl:choose>
					<xsl:when test="type='String'">
						<xsl:text>std::string</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="stdType"/>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>&amp;&gt;(o);&#10;</xsl:text>
				<xsl:text>	deque->push_back(pO);&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- poll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _poll&#10;</xsl:text>
		<xsl:text> * Signature: (J)Lorg/ogre4j/String;&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jstring JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1poll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> &amp;obj = deque->at(0);&#10;</xsl:text>
				<xsl:text>	deque->pop_front();&#10;</xsl:text>
				<xsl:text>	return reinterpret_cast&lt;</xsl:text>
				<xsl:choose>
					<xsl:when test="type='String'">
						<xsl:text>jstring</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>jlong</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>&gt;(&amp;obj);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- remove() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _remove&#10;</xsl:text>
		<xsl:text> * Signature: (J)Lorg/ogre4j/String;&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jstring JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1remove__J&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> &amp;obj = deque->at(0);&#10;</xsl:text>
				<xsl:text>	deque->pop_front();&#10;</xsl:text>
				<xsl:text>	return reinterpret_cast&lt;</xsl:text>
				<xsl:choose>
					<xsl:when test="type='String'">
						<xsl:text>jstring</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>jlong</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>&gt;(&amp;obj);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- peek() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _peek&#10;</xsl:text>
		<xsl:text> * Signature: (J)Lorg/ogre4j/String;&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jstring JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1peek&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> &amp;obj = deque->at(0);&#10;</xsl:text>
				<xsl:text>	return reinterpret_cast&lt;</xsl:text>
				<xsl:choose>
					<xsl:when test="type='String'">
						<xsl:text>jstring</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>jlong</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>&gt;(&amp;obj);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- element() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _element&#10;</xsl:text>
		<xsl:text> * Signature: (J)Lorg/ogre4j/String;&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jstring JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1element&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> str = deque->at(0);&#10;</xsl:text>
				<xsl:choose>
					<xsl:when test="type='String'">
						<xsl:text>	return env->NewStringUTF(str.c_str());&#10;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>	return reinterpret_cast&lt;jlong&gt;(str);&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- size() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _size&#10;</xsl:text>
		<xsl:text> * Signature: (J)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jint JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1size&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	return deque->size();&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- isEmpty() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _isEmpty&#10;</xsl:text>
		<xsl:text> * Signature: (J)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1isEmpty&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	if(deque->empty())&#10;</xsl:text>
				<xsl:text>		return 1;&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- contains() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _contains&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1contains&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> * pO = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> *&gt;(o);&#10;</xsl:text>
				<xsl:text>	for(size_t i=0, size=deque->size(); i&lt;size; i++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> &amp;str = deque->at(i);&#10;</xsl:text>
				<xsl:text>		if(&amp;str==&amp;(*pO))&#10;</xsl:text>
				<xsl:text>			return 1;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- add() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _add&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1add&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> * str = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>*&gt;(o);&#10;</xsl:text>
				<xsl:text>	deque->push_back(*str);&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- remove() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _remove&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1remove__JJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong o)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::deque&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;::iterator it;&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> * str = reinterpret_cast&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>*&gt;(o);&#10;</xsl:text>
				<xsl:text>	for (it=deque->begin(); it!=deque->end(); it++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text> s = *it;&#10;</xsl:text>
				<xsl:text>		if (&amp;s==&amp;(*str))&#10;</xsl:text>
				<xsl:text>		break;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	if (it==deque->end())&#10;</xsl:text>
				<xsl:text>	return 0;&#10;</xsl:text>
				<xsl:text>	deque->erase(it);&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- containsAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _containsAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1containsAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong c)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * collection = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(c);&#10;</xsl:text>
				<xsl:text>	for(size_t i=0, size=collection->size(); i&lt;size; i++)	&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		jlong s = reinterpret_cast&lt;jlong&gt;(&amp;collection->at(i));&#10;</xsl:text>
				<xsl:text>		if(Java_Ogre4J_FileSystemArchive_DirectoryStack__1contains(env, that, pInstance, s)==0)&#10;</xsl:text>
				<xsl:text>			return 0;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- addAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _addAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1addAll__JJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong c)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * collection = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(c);&#10;</xsl:text>
				<xsl:text>	deque->insert(deque->begin(), collection->begin(), collection->end());&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- addAll() 
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _addAll&#10;</xsl:text>
		<xsl:text> * Signature: (JIJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1addAll__JIJ&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jint, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance,jint index, jlong c)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	std::deque * deque = reinterpret_cast<![CDATA[<std::deque*>]]>(pInstance);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>-->
		<!-- removeAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _addAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1removeAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong c)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	deque->clear();&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- retainAll() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _retainAll&#10;</xsl:text>
		<xsl:text> * Signature: (JJ)Z&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT jboolean JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1retainAll&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance, jlong c)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	std::deque&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt; * collection = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text>*&gt;(c);&#10;</xsl:text>
				<xsl:text>	for(size_t i=0, size=deque->size(); i&lt;size; i++)&#10;</xsl:text>
				<xsl:text>	{&#10;</xsl:text>
				<xsl:text>		jlong str = reinterpret_cast&lt;jlong&gt;(&amp;deque->at(i));&#10;</xsl:text>
				<xsl:text>		if(Java_Ogre4J_</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="$typedef"/>
				<xsl:text>__1contains(env, that, c, str)==0)&#10;</xsl:text>
				<xsl.text>		Java_Ogre4J_</xsl.text>
				<xsl:value-of select="$class"/>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="$typedef"/>
				<xsl:text>__1remove__JJ(env, that, pInstance, str);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>	return 1;&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<!-- clear() -->
		<xsl:text>/*&#10;</xsl:text>
		<xsl:text> * Class:     </xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>&#10;</xsl:text>
		<xsl:text> * Method:    _clear&#10;</xsl:text>
		<xsl:text> * Signature: (J)I&#10;</xsl:text>
		<xsl:text> */&#10;</xsl:text>
		<xsl:text>JNIEXPORT void JNICALL Java_</xsl:text>
		<xsl:value-of select="concat($prefix,'_',$class,'_',$typedef)"/>
		<xsl:text>__1clear&#10;</xsl:text>
		<xsl:choose>
			<xsl:when test="$isHeader='true'">
				<xsl:text>  (JNIEnv *, jobject, jlong);&#10;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>  (JNIEnv *env, jobject that, jlong pInstance)&#10;</xsl:text>
				<xsl:text>{&#10;</xsl:text>
				<xsl:text>	</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> * deque = reinterpret_cast&lt;</xsl:text>
				<xsl:call-template name="stdType"/>
				<xsl:text> *&gt;(pInstance);&#10;</xsl:text>
				<xsl:text>	deque->erase(deque->begin());&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- ################################ STDFUNCTIONS ################################ -->
	<xsl:template name="stdFunctions">
		<xsl:param name="baseType"/>
		<xsl:choose>
			<!-- functions for std::map and std::pair and std::multimap -->
			<xsl:when test="$baseType='std::map' or @basetype='std::pair' or contains(@basetype,'std::hash_map') or (@basetype='std::multimap' and not(typedef))">
				<xsl:text>	protected </xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>(InstancePointer pInstance) {&#10;</xsl:text>
				<xsl:text>		super(pInstance);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- dispose() -->
				<xsl:text>	public void dispose() {&#10;</xsl:text>
				<xsl:text>		if (this.createdByLibrary)&#10;</xsl:text>
				<xsl:text>			throw new RuntimeException("can't dispose object created by the library");&#10;</xsl:text>
				<xsl:text>		_dispose(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- size() -->
				<xsl:text>	public int size() {&#10;</xsl:text>
				<xsl:text>		return _size(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- isEmpty() -->
				<xsl:text>	public boolean isEmpty() {&#10;</xsl:text>
				<xsl:text>		return _isEmpty(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- containsKey(Object key) -->
				<xsl:text>	public boolean containsKey(Object key) {&#10;</xsl:text>
				<xsl:text>		return _containsKey(pInstance.value, ((</xsl:text>
				<!-- Key -->
				<xsl:choose>
					<xsl:when test="type=$typedefResultTree/mapping/@type">
						<xsl:variable name="type" select="type[position()=1]"/>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=$type]"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[position()=1]"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>)key).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- containsValue(Object value) -->
				<xsl:text>	public boolean containsValue(Object value) {&#10;</xsl:text>
				<xsl:text>		return _containsValue(pInstance.value, ((</xsl:text>
				<!-- Value -->
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[last()]"/>
				</xsl:call-template>
				<xsl:text>)value).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- get(Object key) -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[last()]"/>
				</xsl:call-template>
				<xsl:text> get(Object key) {&#10;</xsl:text>
				<xsl:choose>
					<xsl:when test="type[last()]='String'">
						<xsl:text>		return _get(pInstance.value, (String)key);&#10;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>		long ptr = _get(pInstance.value, ((</xsl:text>
						<!-- Key -->
						<xsl:choose>
							<xsl:when test="type=$typedefResultTree/mapping/@type">
								<xsl:variable name="type" select="type[position()=1]"/>
								<xsl:call-template name="templateTypeMap">
									<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=$type]"/>
								</xsl:call-template>
							</xsl:when>
							<xsl:otherwise>
								<xsl:call-template name="templateTypeMap">
									<xsl:with-param name="type" select="type[position()=1]"/>
								</xsl:call-template>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:text>)key).pInstance.value);&#10;</xsl:text>
						<xsl:text>		if(ptr==0) return null;&#10;</xsl:text>
						<xsl:text>		InstancePointer ptr</xsl:text>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[last()]"/>
						</xsl:call-template>
						<xsl:text> = new  InstancePointer(ptr);&#10;</xsl:text>
						<xsl:text>		return new </xsl:text>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[last()]"/>
						</xsl:call-template>
						<xsl:text>(ptr</xsl:text>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[last()]"/>
						</xsl:call-template>
						<xsl:text>);&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>	}&#10;</xsl:text>
				<!-- put(Key key, Value value) -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[last()]"/>
				</xsl:call-template>
				<xsl:text> put(</xsl:text>
				<!-- Key -->
				<xsl:choose>
					<xsl:when test="type=$typedefResultTree/mapping/@type">
						<xsl:variable name="type" select="type[position()=1]"/>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=$type]"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[position()=1]"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text> key, </xsl:text>
				<!-- Value -->
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[last()]"/>
				</xsl:call-template>
				<xsl:text> value) {&#10;</xsl:text>
				<xsl:choose>
					<xsl:when test="type[last()]='String'">
						<xsl:text>		return _put(pInstance.value, key, value);&#10;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>		long ptr = _put(pInstance.value, key.pInstance.value, value.pInstance.value);&#10;</xsl:text>
						<xsl:text>		</xsl:text>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[last()]"/>
						</xsl:call-template>
						<xsl:text> old = null;&#10;</xsl:text>
						<xsl:text>		if(ptr>0)&#10;</xsl:text>
						<xsl:text>			old = new </xsl:text>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[last()]"/>
						</xsl:call-template>
						<xsl:text>(new InstancePointer(ptr));&#10;</xsl:text>
						<xsl:text>		return old;&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>	}&#10;</xsl:text>
				<!-- remove(Object key) -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[last()]"/>
				</xsl:call-template>
				<xsl:text> remove(Object key) {&#10;</xsl:text>
				<xsl:choose>
					<xsl:when test="type[last()]='String'">
						<xsl:text>		return _remove(pInstance.value, (String)key);&#10;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>		long ptr = _remove(pInstance.value, ((</xsl:text>
						<!-- Key -->
						<xsl:choose>
							<xsl:when test="type=$typedefResultTree/mapping/@type">
								<xsl:variable name="type" select="type[position()=1]"/>
								<xsl:call-template name="templateTypeMap">
									<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=$type]"/>
								</xsl:call-template>
							</xsl:when>
							<xsl:otherwise>
								<xsl:call-template name="templateTypeMap">
									<xsl:with-param name="type" select="type[position()=1]"/>
								</xsl:call-template>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:text>)key).pInstance.value);&#10;</xsl:text>
						<xsl:text>		if(ptr==0) return null;&#10;</xsl:text>
						<xsl:text>		InstancePointer ptr</xsl:text>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[last()]"/>
						</xsl:call-template>
						<xsl:text> = new  InstancePointer(ptr);&#10;</xsl:text>
						<xsl:text>		return new </xsl:text>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[last()]"/>
						</xsl:call-template>
						<xsl:text>(ptr</xsl:text>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[last()]"/>
						</xsl:call-template>
						<xsl:text>);&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>	}&#10;</xsl:text>
				<!-- putAll(Map<? extends Key, ? extends Value> t) -->
				<xsl:text>	public void putAll(Map<![CDATA[<?]]> extends </xsl:text>
				<xsl:choose>
					<xsl:when test="type=$typedefResultTree/mapping/@type">
						<xsl:variable name="type" select="type[position()=1]"/>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=$type]"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[position()=1]"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>, <![CDATA[?]]> extends </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[last()]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> t) {&#10;</xsl:text>
				<xsl:text>		_putAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)t).pInstance.value);&#10;</xsl:text>
				<xsl:text>}&#10;</xsl:text>
				<!-- clear() -->
				<xsl:text>	public void clear() {&#10;</xsl:text>
				<xsl:text>		_clear(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- keySet() -->
				<xsl:text>	public Set<![CDATA[<]]></xsl:text>
				<xsl:choose>
					<xsl:when test="type=$typedefResultTree/mapping/@type">
						<xsl:variable name="type" select="type[position()=1]"/>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=$type]"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[position()=1]"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text><![CDATA[>]]> keySet() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- values() -->
				<xsl:text>	public Collection<![CDATA[<]]></xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[last()]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> values() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- entrySet() -->
				<xsl:text>	public Set<![CDATA[<]]>Entry<![CDATA[<]]></xsl:text>
				<xsl:choose>
					<xsl:when test="type=$typedefResultTree/mapping/@type">
						<xsl:variable name="type" select="type[position()=1]"/>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=$type]"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[position()=1]"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>, </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[last()]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>>]]> entrySet() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>&#10;</xsl:text>
				<xsl:text>	private native void _dispose(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native int _size(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _isEmpty(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _containsKey(long pInstance, long key);&#10;</xsl:text>
				<xsl:text>	private native boolean _containsValue(long pInstance, long value);&#10;</xsl:text>
				<xsl:choose>
					<xsl:when test="type[last()]='String'">
						<xsl:text>	private native String _get(long pInstance, String key);&#10;</xsl:text>
						<xsl:text>	private native String _put(long pInstance, String key, String value);&#10;</xsl:text>
						<xsl:text>	private native String _remove(long pInstance, String key);&#10;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>	private native long _get(long pInstance, long key);&#10;</xsl:text>
						<xsl:text>	private native long _put(long pInstance, long key, long value);&#10;</xsl:text>
						<xsl:text>	private native long _remove(long pInstance, long key);&#10;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>	private native long _putAll(long pInstance, long t);&#10;</xsl:text>
				<xsl:text>	private native void _clear(long pInstance);&#10;</xsl:text>
			</xsl:when>
			<!-- functions for std::vector and std::list -->
			<xsl:when test="$baseType='std::vector' or $baseType='std::list'">
				<xsl:text>	protected </xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>(InstancePointer pInstance) {&#10;</xsl:text>
				<xsl:text>		super(pInstance);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- dispose() -->
				<xsl:text>	public void dispose() {&#10;</xsl:text>
				<xsl:text>		if (this.createdByLibrary)&#10;</xsl:text>
				<xsl:text>			throw new RuntimeException("can't dispose object created by the library");&#10;</xsl:text>
				<xsl:text>		_dispose(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- size() -->
				<xsl:text>	public int size() {&#10;</xsl:text>
				<xsl:text>		return _size(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- isEmpty() -->
				<xsl:text>	public boolean isEmpty() {&#10;</xsl:text>
				<xsl:text>		return _isEmpty(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- contains(Object o) -->
				<xsl:text>	public boolean contains(Object o) {&#10;</xsl:text>
				<xsl:text>		return _contains(pInstance.value, ((</xsl:text>
				<!-- Key -->
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>)o).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- iterator() -->
				<xsl:text>	public Iterator<![CDATA[<]]></xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> iterator() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- toArray() -->
				<xsl:text>	public Object[] toArray() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- toArray(T[] a) -->
				<xsl:text>	public <![CDATA[<T>]]> T[] toArray(T[] a) {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- add(Value o) -->
				<xsl:text>	public boolean add(</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> o) {&#10;</xsl:text>
				<xsl:text>		return _add(pInstance.value, o.pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- remove(Object o) -->
				<xsl:text>	public boolean remove(Object o) {&#10;</xsl:text>
				<xsl:text>		return _remove(pInstance.value, ((</xsl:text>
				<!-- Key -->
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>)o).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- containsAll(Collection<?> c) -->
				<xsl:text>	public boolean containsAll(Collection<![CDATA[<?>]]> c) { &#10;</xsl:text>
				<xsl:text>		return _containsAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- addAll(Collection<? extends Value> c) -->
				<xsl:text>	public boolean addAll(Collection<![CDATA[<?]]> extends </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> c) {&#10;</xsl:text>
				<xsl:text>		return _addAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- addAll(int index, Collection<? extends Value> c) -->
				<xsl:text>	public boolean addAll(int index, Collection<![CDATA[<?]]> extends </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> c) {&#10;</xsl:text>
				<xsl:text>		return _addAll(pInstance.value, index, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- removeAll(Collection<?> c) -->
				<xsl:text>	public boolean removeAll(Collection<![CDATA[<?>]]> c) { &#10;</xsl:text>
				<xsl:text>		return _removeAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- retainAll(Collection<?> c) -->
				<xsl:text>	public boolean retainAll(Collection<![CDATA[<?>]]> c) { &#10;</xsl:text>
				<xsl:text>		return _retainAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- clear() -->
				<xsl:text>	public void clear() {&#10;</xsl:text>
				<xsl:text>		_clear(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- get(int index) -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> get(int index) {&#10;</xsl:text>
				<xsl:text>		long ptr = _get(pInstance.value, index);&#10;</xsl:text>
				<xsl:text>		if(ptr==0) return null;&#10;</xsl:text>
				<xsl:text>		InstancePointer ptr</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> = new  InstancePointer(ptr);&#10;</xsl:text>
				<xsl:text>		return new </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>(ptr</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- set(int index, Value element) -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> set(int index, </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> element) {&#10;</xsl:text>
				<xsl:text>		long ptr = _set(pInstance.value, index, element.pInstance.value);&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> old = null;&#10;</xsl:text>
				<xsl:text>		if(ptr>0)&#10;</xsl:text>
				<xsl:text>			old = new </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>(new InstancePointer(ptr));&#10;</xsl:text>
				<xsl:text>		return old;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- add(int index, Value element) -->
				<xsl:text>	public void add(int index, </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> element) {&#10;</xsl:text>
				<xsl:text>		_add(pInstance.value, index, element.pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- remove(int index) -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> remove(int index) {&#10;</xsl:text>
				<xsl:text>		long ptr = _remove(pInstance.value, index);&#10;</xsl:text>
				<xsl:text>		</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> old = null;&#10;</xsl:text>
				<xsl:text>		if(ptr>0)&#10;</xsl:text>
				<xsl:text>			old = new </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>(new InstancePointer(ptr));&#10;</xsl:text>
				<xsl:text>		return old;&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- indexOf(Object o) -->
				<xsl:text>	public int indexOf(Object o) {&#10;</xsl:text>
				<xsl:text>		return _indexOf(pInstance.value,((</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>)o).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- lastIndexOf(Object o) -->
				<xsl:text>	public int lastIndexOf(Object o) {&#10;</xsl:text>
				<xsl:text>		return _lastIndexOf(pInstance.value,((</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>)o).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- listIterator() -->
				<xsl:text>	public ListIterator<![CDATA[<]]></xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> listIterator() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- listIterator(int index) -->
				<xsl:text>	public ListIterator<![CDATA[<]]></xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> listIterator(int index) {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- subList(int fromIndex, int toIndex) -->
				<xsl:text>	public List<![CDATA[<]]></xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> subList(int fromIndex, int toIndex) {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>&#10;</xsl:text>
				<xsl:text>	private native void _dispose(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native int _size(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _isEmpty(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _contains(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _add(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _remove(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _containsAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _addAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _addAll(long pInstance, int index, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _removeAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _retainAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native void _clear(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native long _get(long pInstance, int index);&#10;</xsl:text>
				<xsl:text>	private native long _set(long pInstance, int index, long element);&#10;</xsl:text>
				<xsl:text>	private native long _add(long pInstance, int index, long element);&#10;</xsl:text>
				<xsl:text>	private native long _remove(long pInstance, int index);&#10;</xsl:text>
				<xsl:text>	private native int _indexOf(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native int _lastIndexOf(long pInstance, long o);&#10;</xsl:text>
			</xsl:when>
			<!-- functions for std::set -->
			<xsl:when test="@basetype='std::set'">
				<xsl:text>	protected </xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>(InstancePointer pInstance) {&#10;</xsl:text>
				<xsl:text>		super(pInstance);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- dispose() -->
				<xsl:text>	public void dispose() {&#10;</xsl:text>
				<xsl:text>		if (this.createdByLibrary)&#10;</xsl:text>
				<xsl:text>			throw new RuntimeException("can't dispose object created by the library");&#10;</xsl:text>
				<xsl:text>		_dispose(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- size() -->
				<xsl:text>	public int size() {&#10;</xsl:text>
				<xsl:text>		return _size(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- isEmpty() -->
				<xsl:text>	public boolean isEmpty() {&#10;</xsl:text>
				<xsl:text>		return _isEmpty(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- contains(Object o) -->
				<xsl:text>	public boolean contains(Object o) {&#10;</xsl:text>
				<xsl:text>		return _contains(pInstance.value, ((</xsl:text>
				<!-- Key -->
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>)o).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- iterator() -->
				<xsl:text>	public Iterator<![CDATA[<]]></xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> iterator() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- toArray() -->
				<xsl:text>	public Object[] toArray() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- toArray(T[] a) -->
				<xsl:text>	public <![CDATA[<T>]]> T[] toArray(T[] a) {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- add(Value o) -->
				<xsl:text>	public boolean add(</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> o) {&#10;</xsl:text>
				<xsl:text>		return _add(pInstance.value, o.pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- remove(Object o) -->
				<xsl:text>	public boolean remove(Object o) {&#10;</xsl:text>
				<xsl:text>		return _remove(pInstance.value, ((</xsl:text>
				<!-- Key -->
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>)o).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- containsAll(Collection<?> c) -->
				<xsl:text>	public boolean containsAll(Collection<![CDATA[<?>]]> c) { &#10;</xsl:text>
				<xsl:text>		return _containsAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- addAll(Collection<? extends Value> c) -->
				<xsl:text>	public boolean addAll(Collection<![CDATA[<?]]> extends </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> c) {&#10;</xsl:text>
				<xsl:text>		return _addAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- addAll(int index, Collection<? extends Value> c) 
				<xsl:text>	public boolean addAll(int index, Collection<![CDATA[<?]]> extends </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> c) {&#10;</xsl:text>
				<xsl:text>		return _addAll(pInstance.value, index, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>-->
				<!-- removeAll(Collection<?> c) -->
				<xsl:text>	public boolean removeAll(Collection<![CDATA[<?>]]> c) { &#10;</xsl:text>
				<xsl:text>		return _removeAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- retainAll(Collection<?> c) -->
				<xsl:text>	public boolean retainAll(Collection<![CDATA[<?>]]> c) { &#10;</xsl:text>
				<xsl:text>		return _retainAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- clear() -->
				<xsl:text>	public void clear() {&#10;</xsl:text>
				<xsl:text>		_clear(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>&#10;</xsl:text>
				<xsl:text>	private native void _dispose(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native int _size(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _isEmpty(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _contains(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _add(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _remove(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _containsAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _addAll(long pInstance, long o);&#10;</xsl:text>
				<!--<xsl:text>	private native boolean _addAll(long pInstance, int index, long o);&#10;</xsl:text>-->
				<xsl:text>	private native boolean _removeAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _retainAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native int _clear(long pInstance);&#10;</xsl:text>
			</xsl:when>
			<!-- functions for std::deque -->
			<xsl:when test="@basetype='std::deque'">
				<xsl:text>	protected </xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>(InstancePointer pInstance) {&#10;</xsl:text>
				<xsl:text>		super(pInstance);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- dispose() -->
				<xsl:text>	public void dispose() {&#10;</xsl:text>
				<xsl:text>		if (this.createdByLibrary)&#10;</xsl:text>
				<xsl:text>			throw new RuntimeException("can't dispose object created by the library");&#10;</xsl:text>
				<xsl:text>		_dispose(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- offer(String o) -->
				<xsl:text>	public boolean offer(</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> o) {&#10;</xsl:text>
				<xsl:text>		return _offer(pInstance.value, o);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- poll() -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> poll() {&#10;</xsl:text>
				<xsl:text>		return _poll(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- remove() -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> remove() {&#10;</xsl:text>
				<xsl:text>		return _remove(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- peek() -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> peek() {&#10;</xsl:text>
				<xsl:text>		return _peek(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- element() -->
				<xsl:text>	public </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> element() {&#10;</xsl:text>
				<xsl:text>		return _element(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- size() -->
				<xsl:text>	public int size() {&#10;</xsl:text>
				<xsl:text>		return _size(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- isEmpty() -->
				<xsl:text>	public boolean isEmpty() {&#10;</xsl:text>
				<xsl:text>		return _isEmpty(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- contains(Object o) -->
				<xsl:text>	public boolean contains(Object o) {&#10;</xsl:text>
				<xsl:text>		return _contains(pInstance.value, ((</xsl:text>
				<!-- Key -->
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>)o).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- iterator() -->
				<xsl:text>	public Iterator<![CDATA[<]]></xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> iterator() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- toArray() -->
				<xsl:text>	public Object[] toArray() {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- toArray(T[] a) -->
				<xsl:text>	public <![CDATA[<T>]]> T[] toArray(T[] a) {&#10;</xsl:text>
				<xsl:text>		throw new UnsupportedOperationException();&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- add(Value o) -->
				<xsl:text>	public boolean add(</xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text> o) {&#10;</xsl:text>
				<xsl:text>		return _add(pInstance.value, o.pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- remove(Object o) -->
				<xsl:text>	public boolean remove(Object o) {&#10;</xsl:text>
				<xsl:text>		return _remove(pInstance.value, ((</xsl:text>
				<!-- Key -->
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text>)o).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- containsAll(Collection<?> c) -->
				<xsl:text>	public boolean containsAll(Collection<![CDATA[<?>]]> c) { &#10;</xsl:text>
				<xsl:text>		return _containsAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- addAll(Collection<? extends Value> c) -->
				<xsl:text>	public boolean addAll(Collection<![CDATA[<?]]> extends </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> c) {&#10;</xsl:text>
				<xsl:text>		return _addAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- addAll(int index, Collection<? extends Value> c) 
				<xsl:text>	public boolean addAll(int index, Collection<![CDATA[<?]]> extends </xsl:text>
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[position()=1]"/>
				</xsl:call-template>
				<xsl:text><![CDATA[>]]> c) {&#10;</xsl:text>
				<xsl:text>		return _addAll(pInstance.value, index, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text> -->
				<!-- removeAll(Collection<?> c) -->
				<xsl:text>	public boolean removeAll(Collection<![CDATA[<?>]]> c) { &#10;</xsl:text>
				<xsl:text>		return _removeAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- retainAll(Collection<?> c) -->
				<xsl:text>	public boolean retainAll(Collection<![CDATA[<?>]]> c) { &#10;</xsl:text>
				<xsl:text>		return _retainAll(pInstance.value, ((</xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>)c).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- clear() -->
				<xsl:text>	public void clear() {&#10;</xsl:text>
				<xsl:text>		_clear(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<xsl:text>&#10;</xsl:text>
				<xsl:text>	private native void _dispose(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _offer(long pInstance, String o);&#10;</xsl:text>
				<xsl:text>	private native String _poll(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native String _remove(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native String _peek(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native String _element(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native int _size(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _isEmpty(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _contains(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _add(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _remove(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _containsAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _addAll(long pInstance, long o);&#10;</xsl:text>
				<!--<xsl:text>	private native boolean _addAll(long pInstance, int index, long o);&#10;</xsl:text>-->
				<xsl:text>	private native boolean _removeAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native boolean _retainAll(long pInstance, long o);&#10;</xsl:text>
				<xsl:text>	private native void _clear(long pInstance);&#10;</xsl:text>
			</xsl:when>
			<!-- functions for std::hash_compare and std::multimap-->
			<xsl:when test="@basetype='stdext::hash_compare' or @basetype='std::multimap'">
				<xsl:text>	protected </xsl:text>
				<xsl:value-of select="@name"/>
				<xsl:text>(InstancePointer pInstance) {&#10;</xsl:text>
				<xsl:text>		super(pInstance);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- dispose() -->
				<xsl:text>	public void dispose() {&#10;</xsl:text>
				<xsl:text>		if (this.createdByLibrary)&#10;</xsl:text>
				<xsl:text>			throw new RuntimeException("can't dispose object created by the library");&#10;</xsl:text>
				<xsl:text>		_dispose(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- size() -->
				<xsl:text>	public int size() {&#10;</xsl:text>
				<xsl:text>		return _size(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- isEmpty() -->
				<xsl:text>	public boolean isEmpty() {&#10;</xsl:text>
				<xsl:text>		return _isEmpty(pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- containsKey(Object key) -->
				<xsl:text>	public boolean containsKey(Object key) {&#10;</xsl:text>
				<xsl:text>		return _containsKey(pInstance.value, ((</xsl:text>
				<!-- Key -->
				<xsl:choose>
					<xsl:when test="type=$typedefResultTree/mapping/@type">
						<xsl:variable name="type" select="type[position()=1]"/>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="$typedefResultTree/mapping[@type=$type]"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="type[position()=1]"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>)key).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- containsValue(Object value) -->
				<xsl:text>	public boolean containsValue(Object value) {&#10;</xsl:text>
				<xsl:text>		return _containsValue(pInstance.value, ((</xsl:text>
				<!-- Value -->
				<xsl:call-template name="templateTypeMap">
					<xsl:with-param name="type" select="type[last()]"/>
				</xsl:call-template>
				<xsl:text>)value).pInstance.value);&#10;</xsl:text>
				<xsl:text>	}&#10;</xsl:text>
				<!-- get(Object key) -->
				<xsl:text>	public </xsl:text>
				<xsl:for-each select="typedef[position()=1]">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text> get(Object arg0) { return null; }&#10;</xsl:text>
				<!-- put(Key key, Value value) -->
				<xsl:text>	public </xsl:text>
				<xsl:for-each select="typedef[position()=1]">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text> put(</xsl:text>
				<xsl:for-each select="typedef[position()=1]">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text> arg0, </xsl:text>
				<xsl:for-each select="typedef[position()=2]">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text> arg1) { return null; }&#10;</xsl:text>
				<xsl:text>	public </xsl:text>
				<xsl:for-each select="typedef[position()=1]">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text> remove(Object arg0) { return null; }&#10;</xsl:text>
				<xsl:text>	public void putAll(Map<![CDATA[<?]]> extends </xsl:text>
				<xsl:for-each select="typedef[position()=1]">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>, <![CDATA[?]]> extends </xsl:text>
				<xsl:for-each select="typedef[position()=1]">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>> arg0) { }&#10;</xsl:text>
				<xsl:text>	public void clear() { }&#10;</xsl:text>
				<xsl:text>	public Set<![CDATA[<]]></xsl:text>
				<xsl:for-each select="typedef[position()=1]">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>> keySet() { return null; }&#10;</xsl:text>
				<xsl:text>	public Collection<![CDATA[<]]></xsl:text>
				<xsl:for-each select="typedef[position()=1]">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>> values() { return null; }&#10;</xsl:text>
				<xsl:text>	public Set<![CDATA[<Entry<]]></xsl:text>
				<xsl:for-each select="typedef">
					<xsl:call-template name="STLMap"/>
					<xsl:text>&lt;</xsl:text>
					<xsl:for-each select="type">
						<xsl:call-template name="templateTypeMap">
							<xsl:with-param name="type" select="."/>
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>&gt;</xsl:text>
					<xsl:if test="position()!=last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text><![CDATA[>>]]> entrySet() { return null; }&#10;</xsl:text>
				<xsl:text>&#10;</xsl:text>
				<xsl:text>	private native void _dispose(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native int _size(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _isEmpty(long pInstance);&#10;</xsl:text>
				<xsl:text>	private native boolean _containsKey(long pInstance, long key);&#10;</xsl:text>
				<xsl:text>	private native boolean _containsValue(long pInstance, long value);&#10;</xsl:text>
			</xsl:when>
			<xsl:when test="count(type)=1">
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>	public void dispose() { }&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- ################################ ITERATORFUNCTIONS ################################ -->
	<xsl:template name="iteratorFunctions">
		<xsl:text>	public void dispose() { }&#10;</xsl:text>
		<xsl:if test="@basetype!=''">
			<xsl:text>	public boolean hasNext() { return false; }&#10;</xsl:text>
			<xsl:text>	public </xsl:text>
			<xsl:choose>
				<xsl:when test="contains(@basetype,'Ogre::')">
					<xsl:value-of select="substring-after(@basetype,'Ogre::')"/>
				</xsl:when>
				<!-- mapping of Real to NativeReal, can also take float etc. when appearing -->
				<xsl:when test="contains(@basetype,'Real') and not(@type=root()//typedef/@name)">
					<xsl:variable name="nativeType" select="concat('Native',@type)"/>
					<xsl:value-of select="replace(@basetype,@type,$nativeType)"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@basetype"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:text> next() { return null; }&#10;</xsl:text>
			<xsl:text>	public void remove() { }&#10;</xsl:text>
		</xsl:if>
	</xsl:template>
	<!-- ######################## NATIVEFUNCSIGNATURE ######################## -->
	<xsl:template name="nativeFuncSignature">
		<xsl:param name="calledFrom"/>
		<xsl:choose>
			<xsl:when test="$calledFrom='cHeader'">
				<xsl:text>1</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>_</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:for-each select="parameters/parameter">
			<xsl:choose>
				<xsl:when test="contains(type,'.')">
					<xsl:value-of select="replace(type,'.','')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="type"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>
	<!-- #### STDTYPE #### -->
	<xsl:template name="stdType">
		<xsl:variable name="namespace" select="../../@name"/>
		<xsl:variable name="class" select="../@name"/>
		<xsl:variable name="typedef" select="@name"/>
		<xsl:choose>
			<!-- public typedefs -->
			<xsl:when test="@protection='public'">
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$class"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="$typedef"/>
			</xsl:when>
			<!-- map, pair, multimap -->
			<xsl:when test="@basetype='std::map' or @basetype='std::pair' or @basetype='std::multimap'">
				<xsl:text>std::map&lt;</xsl:text>
				<xsl:if test="not(contains($simpleTypes,type[position()=1])) or type[position()=1]='String'">
					<xsl:value-of select="$namespace"/>
					<xsl:text>::</xsl:text>
				</xsl:if>
				<xsl:value-of select="type[position()=1]"/>
				<xsl:text>, </xsl:text>
				<xsl:if test="not(contains($simpleTypes,type[position()=2])) or type[position()=2]='String'">
					<xsl:value-of select="$namespace"/>
					<xsl:text>::</xsl:text>
				</xsl:if>
				<xsl:value-of select="type[position()=2]"/>
				<xsl:text>&gt;</xsl:text>
			</xsl:when>
			<!-- vector, list -->
			<xsl:when test="@basetype='std::vector' or @basetype='std::list'">
				<xsl:text>std::vector&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;</xsl:text>
			</xsl:when>
			<!-- set -->
			<xsl:when test="@basetype='std::set'">
				<xsl:text>std::set&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;</xsl:text>
			</xsl:when>
			<!-- deque -->
			<xsl:when test="@basetype='std::deque'">
				<xsl:text>std::deque&lt;</xsl:text>
				<xsl:value-of select="$namespace"/>
				<xsl:text>::</xsl:text>
				<xsl:value-of select="type"/>
				<xsl:text>&gt;</xsl:text>
			</xsl:when>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
<!--
For using the <xsl:result-document href="{$filename}">, XSLT 2.0 is required
XSLT 2.0 is supported by the Saxon processor, usage:
java -jar "C:/Dokumente und Einstellungen/rung/Eigene Dateien/saxon8.2/saxon8.jar" meta.xml output.xslt
-->
