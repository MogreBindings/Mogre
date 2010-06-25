<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xd="http://www.pnp-software.com/XSLTdoc" version="2.0">
	<xd:doc type="stylesheet">
		<xd:author>Hubert Rung</xd:author>
		<xd:copyright>netAllied GmbH</xd:copyright>
		<xd:cvsId>$Id: XSLTdocConfig.xml,v 1.21 2005/07/13 19:48:59 ibirrer Exp $</xd:cvsId>
	</xd:doc>
	<xsl:output method="xml" version="1.0" encoding="iso-8859-1" indent="yes"/>
	<!-- ############################## MAIN - calls namespace ############################## -->
	<xsl:template match="/">
		<xsl:element name="meta">
			<xsl:for-each select="doxygen/compounddef[@kind='namespace']">
				<xsl:call-template name="namespace"/>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>
	<!-- ############### NAMESPACE - calls class; globals - calls typedef, enum, function, variable ################# -->
	<!-- cursor on doxygen/compounddef -->
	<xsl:template name="namespace">
		<xsl:element name="namespace">
			<xsl:attribute name="name" select="compoundname"/>
			<xsl:variable name="name" select="compoundname"/>
			<xsl:if test="contains($name,'::')">
				<xsl:attribute name="name" select="substring-before($name,'::')"/>
				<xsl:attribute name="second" select="substring-after($name,'::')"/>
			</xsl:if>
			<!-- classes -->
			<xsl:for-each select="innerclass">
				<xsl:call-template name="class">
					<xsl:with-param name="refid" select="@refid"/>
				</xsl:call-template>
			</xsl:for-each>
			<!-- global typedefs -->
			<xsl:for-each select="sectiondef[@kind='typedef']">
				<xsl:call-template name="typedef"/>
			</xsl:for-each>
			<!-- global enums -->
			<xsl:for-each select="sectiondef[@kind='enum']">
				<xsl:call-template name="enumeration"/>
			</xsl:for-each>
			<!-- global functions -->
			<xsl:for-each select="sectiondef[@kind='func']">
				<xsl:if test="not(memberdef/templateparamlist)">
					<xsl:call-template name="function"/>
				</xsl:if>
			</xsl:for-each>
			<!-- global variables -->
			<xsl:for-each select="sectiondef[@kind='var']">
				<xsl:call-template name="variable"/>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>
	<!-- ############### CLASS - calls struct, derivation, enumeration, typedef, function ################## -->
	<!-- cursor on doxygen/compounddef/innerclass -->
	<xsl:template name="class">
		<xsl:param name="refid"/>
		<!-- structs -->
		<xsl:for-each select="root()//compounddef[@id=$refid and @kind='struct']/sectiondef[@kind='public-attrib']">
			<xsl:call-template name="struct"/>
		</xsl:for-each>
		<!-- classes -->
		<xsl:for-each select="root()//compounddef[@id=$refid and @kind='class']">
			<xsl:element name="class">
				<xsl:attribute name="name" select="substring-after(compoundname,'::')"/>
				<xsl:attribute name="fullName" select="compoundname"/>

				<!-- PIPAAAAAA-->
				<xsl:attribute name="protection" select="@prot"/>
				<xsl:attribute name="includeFile" select="includes"/>

				<!-- template attributes -->
				<xsl:if test="templateparamlist">
					<xsl:attribute name="template" select="'true'"/>
					<xsl:attribute name="templateType" select="templateparamlist/param/type"/>
					<xsl:attribute name="templateDeclaration" select="templateparamlist/param/declname"/>
					<xsl:for-each select="inheritancegraph/node">
						<xsl:if test="link/@refid=$refid">
							<xsl:variable name="childnode" select="childnode/@refid"/>
							<xsl:for-each select="../node">
								<xsl:if test="@id=$childnode">
									<xsl:attribute name="templateName"><xsl:variable name="string" select="substring-before(label,' &gt;')"/><xsl:value-of select="substring-after($string,'&lt; ')"/></xsl:attribute>
								</xsl:if>
							</xsl:for-each>
						</xsl:if>
					</xsl:for-each>
				</xsl:if>
				<!-- getting the right name of inner classes -->
				<xsl:variable name="name" select="substring-after(compoundname,'::')"/>
				<xsl:if test="contains($name,'::')">
					<xsl:attribute name="name" select="substring-after($name,'::')"/>
					<xsl:variable name="name" select="substring-after($name,'::')"/>
					<xsl:if test="contains($name,'::')">
						<xsl:attribute name="name" select="substring-after($name,'::')"/>
						<xsl:variable name="name" select="substring-after($name,'::')"/>
						<xsl:if test="contains($name,'::')">
							<xsl:attribute name="name" select="substring-after($name,'::')"/>
						</xsl:if>
					</xsl:if>
				</xsl:if>
				<!-- cursor on doxygen/compounddef[@kind="class"] -->
				<!-- derivation -->
				<xsl:call-template name="derivation">
					<xsl:with-param name="refid" select="$refid"/>
				</xsl:call-template>
				<!-- enumerations -->
				<xsl:for-each select="sectiondef[@kind='public-type' or @kind='protected-type']">
					<xsl:choose>
						<xsl:when test="memberdef[@kind='enum']">
							<xsl:call-template name="enumeration"/>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>
				<!-- typedefs -->
				<xsl:for-each select="sectiondef[@kind='public-type' or @kind='protected-type' or @kind='private-type']">
					<xsl:choose>
						<xsl:when test="memberdef[@kind='typedef']">
							<xsl:call-template name="typedef"/>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>
				<!-- public functions -->
				<xsl:for-each select="sectiondef[@kind='public-func']">
					<xsl:choose>
						<xsl:when test="memberdef[@kind='function']">
							<xsl:call-template name="function">
							</xsl:call-template>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>

				<!-- static functions -->
				<xsl:for-each select="sectiondef[@kind='public-static-func']">
					<xsl:choose>
						<xsl:when test="memberdef[@kind='function']">
							<xsl:call-template name="function">
							</xsl:call-template>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>
				<!-- recursion for inner classes -->
				<xsl:for-each select="innerclass">
					<xsl:call-template name="class">
						<xsl:with-param name="refid" select="@refid"/>
					</xsl:call-template>
				</xsl:for-each>

				<!-- PIPAAAAAAAAAA -->
				<!-- static variables -->
				<xsl:for-each select="sectiondef[@kind='public-static-attrib']">
					<xsl:choose>
						<xsl:when test="memberdef[@kind='variable']">
							<xsl:call-template name="variable">
							</xsl:call-template>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>

				<!-- PIPAAAAAAAAAA -->
				<!-- public variables -->
				<xsl:for-each select="sectiondef[@kind='public-attrib']">
					<xsl:choose>
						<xsl:when test="memberdef[@kind='variable']">
							<xsl:call-template name="variable">
							</xsl:call-template>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>

				<!-- PIPAAAAAAAAAA -->
				<!-- protected functions -->
				<xsl:for-each select="sectiondef[@kind='protected-func']">
					<xsl:choose>
						<xsl:when test="memberdef[@kind='function']">
							<xsl:call-template name="function">
							</xsl:call-template>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>

				<!-- PIPAAAAAAAAAA -->
				<!-- protected variables -->
				<xsl:for-each select="sectiondef[@kind='protected-attrib']">
					<xsl:choose>
						<xsl:when test="memberdef[@kind='variable']">
							<xsl:call-template name="variable">
							</xsl:call-template>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>

			</xsl:element>
		</xsl:for-each>
	</xsl:template>
	<!-- ##################### DERIVATION ######################### -->
	<!-- cursor on doxygen/compounddef -->
	<xsl:template name="derivation">
		<xsl:param name="refid"/>
		<xsl:if test="derivedcompoundref">
			<xsl:element name="derives">
				<xsl:for-each select="derivedcompoundref">
					<xsl:element name="subClass">
						<xsl:value-of select="substring-after(@refid,'_1_1')"/>
					</xsl:element>
				</xsl:for-each>
			</xsl:element>
		</xsl:if>
		<xsl:if test="basecompoundref">
			<xsl:element name="inherits">
				<!--
				<xsl:for-each select="basecompoundref">
					 sort elements beginning with fewest virtual functions 
					<xsl:sort select="count(root()//compounddef[@id=current()/@refid]/sectiondef[@kind='public-func']/memberdef[@virt='pure-virtual'])" order="ascending" data-type="number"/>
					<xsl:element name="baseClass">
						<xsl:value-of select="substring-after(@refid,'_1_1')"/>
					</xsl:element>
				</xsl:for-each>
				-->

				<!-- PIPAAAAAAAAAA -->
				<xsl:if test="basecompoundref[@refid='structCLRObject']">
					<xsl:element name="baseClass">
						<xsl:value-of select="'CLRObject'"/>
					</xsl:element>
				</xsl:if>

				<xsl:for-each select="inheritancegraph/node[link/@refid=$refid]">
					<xsl:for-each select="childnode">
						<xsl:variable name="childid" select="@refid"/>
						<!-- non templates first -->
						<xsl:if test="not(contains(substring-after(../../node[@id=$childid]/label,'::'),'&lt;'))">
							<xsl:element name="baseClass">
								<xsl:variable name="baseClass" select="substring-after(../../node[@id=$childid]/label,'::')"/>
								<xsl:choose>
									<xsl:when test="contains($baseClass,'::')">
										<xsl:value-of select="$baseClass"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="$baseClass"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>
						</xsl:if>
					</xsl:for-each>
					<xsl:for-each select="childnode">
						<xsl:variable name="childid" select="@refid"/>
						<!-- templates -->
						<xsl:if test="contains(substring-after(../../node[@id=$childid]/label,'::'),'&lt;')">
							<xsl:element name="baseClass">
								<xsl:variable name="baseClass" select="substring-after(../../node[@id=$childid]/label,'::')"/>
								<xsl:choose>
									<xsl:when test="contains($baseClass,'::')">
										<xsl:value-of select="$baseClass"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="$baseClass"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>
						</xsl:if>
					</xsl:for-each>
				</xsl:for-each>
			</xsl:element>
		</xsl:if>
	</xsl:template>
	<!-- ##################### ENUMERATION ######################### -->
	<!-- cursor on doxygen/compounddef/sectiondef -->
	<xsl:template name="enumeration">
		<xsl:for-each select="memberdef[@kind='enum']">
			<!-- test if enum belongs to actual class -->
			<xsl:if test="starts-with(@id,../../@id)">
				<xsl:element name="enumeration">
					<xsl:attribute name="name" select="name"/>

					<!-- PIPAAAAA-->
					<xsl:attribute name="protection" select="@prot"/>
					<xsl:attribute name="includeFile" select="substring-after(location/@file,'/ogremain/include/')"/>

					<xsl:for-each select="enumvalue">
						<xsl:element name="enum">
							<xsl:attribute name="name" select="name"/>
						</xsl:element>
					</xsl:for-each>
				</xsl:element>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>
	<!-- ##################### TYPEDEF - calls typeMap ######################### -->
	<!-- cursor on doxygen/compounddef/sectiondef -->
	<xsl:template name="typedef">
		<xsl:for-each select="memberdef[@kind='typedef']">
			<xsl:element name="typedef">
				<xsl:attribute name="name" select="name"/>
				<xsl:attribute name="protection" select="@prot"/>

				<!-- PIPAAAAA-->
				<xsl:attribute name="includeFile" select="substring-after(location/@file,'/ogremain/include/')"/>

				<xsl:choose>
					<xsl:when test="contains(type,'std::') or contains(type,',')">
						<xsl:attribute name="basetype" select="substring-before(type,'&lt;')"/>
						<xsl:variable name="basetype" select="substring-before(type,'&lt;')"/>
						<xsl:if test="contains($basetype,'&#xA;')">
							<xsl:attribute name="basetype" select="normalize-space(substring-after($basetype,'&#xA;'))"/>
						</xsl:if>
						<xsl:variable name="string1" select="substring-after(type,'&lt; ')"/>
						<xsl:choose>
							<!-- e.g. std::multimap< std::pair<size_t, size_t>, std::pair<size_t, size_t> > -->
							<xsl:when test="starts-with($string1,'std::')">
								<xsl:variable name="type1" select="substring-before($string1,', std::')"/>
								<xsl:element name="typedef">
									<xsl:attribute name="basetype" select="substring-before($type1,'&lt;')"/>
									<xsl:variable name="string1" select="substring-after($type1,'&lt; ')"/>
									<xsl:call-template name="typeMap">
										<xsl:with-param name="type" select="normalize-space(substring-before($string1,','))"/>
									</xsl:call-template>
									<xsl:variable name="string2" select="substring-before($string1,' &gt;')"/>
									<xsl:call-template name="typeMap">
										<xsl:with-param name="type" select="normalize-space(substring-after($string2,','))"/>
									</xsl:call-template>
								</xsl:element>
								<xsl:variable name="type2" select="substring-after($string1,'&gt;, ')"/>
								<xsl:element name="typedef">
									<xsl:attribute name="basetype" select="substring-before($type2,'&lt;')"/>
									<xsl:variable name="string1" select="substring-after($type2,'&lt; ')"/>
									<xsl:call-template name="typeMap">
										<xsl:with-param name="type" select="normalize-space(substring-before($string1,','))"/>
									</xsl:call-template>
									<xsl:variable name="string2" select="substring-before($string1,' &gt;')"/>
									<xsl:call-template name="typeMap">
										<xsl:with-param name="type" select="normalize-space(substring-after($string2,','))"/>
									</xsl:call-template>
								</xsl:element>
							</xsl:when>
							<xsl:when test="contains($string1,',')">
								<xsl:call-template name="typeMap">
									<xsl:with-param name="type" select="substring-before($string1,',')"/>
								</xsl:call-template>
								<xsl:variable name="string2" select="substring-after(type,', ')"/>
								<xsl:choose>
									<!-- e.g. std::map<Vector3, size_t, vectorLess> -->
									<xsl:when test="contains($string2,',')">
										<xsl:call-template name="typeMap">
											<xsl:with-param name="type" select="normalize-space(substring-before($string2,','))"/>
										</xsl:call-template>
									</xsl:when>
									<!-- e.g. std::map<Vector3, size_t> -->
									<xsl:otherwise>
										<xsl:call-template name="typeMap">
											<xsl:with-param name="type" select="substring-before($string2,'&gt;')"/>
										</xsl:call-template>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:otherwise>
								<xsl:call-template name="type">
									<xsl:with-param name="type" select="substring-before($string1,'&gt;')"/>
								</xsl:call-template>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<!-- iterator, simple typedefs -->
					<xsl:otherwise>
						<xsl:attribute name="basetype" select="type"/>
						<xsl:variable name="basetype" select="type"/>
						<xsl:if test="contains($basetype,'&#xA;')">
							<xsl:attribute name="basetype" select="normalize-space(substring-after($basetype,'&#xA;'))"/>
						</xsl:if>
						<xsl:choose>
							<xsl:when test="type/ref[@kindref='member']">
								<xsl:choose>
									<xsl:when test="contains(type/ref[@kindref='member'],'::')">
										<xsl:attribute name="type" select="substring-after(type/ref[@kindref='member'],'::')"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:attribute name="type" select="type/ref[@kindref='member']"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:when test="type/ref[@kindref='compound']">
								<xsl:choose>
									<xsl:when test="count(type/ref[@kindref='compound'])=2">
										<xsl:attribute name="type" select="type/ref[@kindref='compound' and position()=2]"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:variable name="string1" select="substring-after(type,'&lt;')"/>
										<xsl:attribute name="type" select="normalize-space(substring-before($string1,'&gt;'))"/>
									</xsl:otherwise>
									<!--
									<xsl:otherwise>
										<xsl:attribute name="type" select="type/ref[@kindref='compound']"/>
									</xsl:otherwise>
									-->
								</xsl:choose>
							</xsl:when>
						</xsl:choose>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
		</xsl:for-each>
	</xsl:template>
	<!-- ##################### STRUCT - calls typeMap ######################### -->
	<!-- cursor on doxygen/compounddef/sectiondef -->
	<xsl:template name="struct">
		<xsl:element name="struct">
			<xsl:attribute name="name" select="substring-after(../compoundname,'::')"/>
			<xsl:variable name="name" select="substring-after(../compoundname,'::')"/>
			<xsl:if test="contains($name,'::')">
				<xsl:attribute name="name" select="substring-after($name,'::')"/>
			</xsl:if>
			<!-- template attributes -->
			<xsl:if test="../templateparamlist">
				<xsl:variable name="refid" select="../@id"/>
				<xsl:attribute name="template" select="'true'"/>
				<xsl:attribute name="templateType" select="../templateparamlist/param/type"/>
				<xsl:attribute name="templateDeclaration" select="../templateparamlist/param/declname"/>
				<xsl:for-each select="../collaborationgraph/node">
					<xsl:if test="link/@refid=$refid">
						<xsl:variable name="childnode" select="childnode/@refid"/>
						<xsl:for-each select="../node">
							<xsl:if test="@id=$childnode">
								<xsl:attribute name="templateName" select="label"/>
							</xsl:if>
						</xsl:for-each>
					</xsl:if>
				</xsl:for-each>
			</xsl:if>
			<!-- struct attributes -->
			<xsl:for-each select="memberdef[@kind='variable']">

				<!-- PIPAAAAAAAAAA -->
				<xsl:element name="variable">

					<xsl:attribute name="protection" select="@prot"/>
					<xsl:attribute name="static" select="@static"/>

					<xsl:if test="type and type!='virtual'">
						<xsl:call-template name="type"/>
					</xsl:if>
					<xsl:element name="definition">
						<xsl:value-of select="definition"/>
					</xsl:element>

					<xsl:element name="name">
						<xsl:value-of select="name"/>
					</xsl:element>

					<!--  <xsl:call-template name="typeMap">
						<xsl:with-param name="type" select="type"/>
					</xsl:call-template> -->

				</xsl:element>
			</xsl:for-each>
			<!-- struct functions -->
			<xsl:for-each select="../sectiondef[@kind='public-func']">
				<xsl:call-template name="function"/>
			</xsl:for-each>
			<!--
			<xsl:for-each select="../sectiondef[@kind='public-func']/memberdef[@kind='function']">
				<xsl:element name="function">
					<xsl:element name="type">
						<xsl:value-of select="type"/>
					</xsl:element>
					<xsl:element name="definition">
						<xsl:value-of select="definition"/>
					</xsl:element>
					<xsl:element name="name">
						<xsl:value-of select="name"/>
					</xsl:element>
					<xsl:if test="param">
						<xsl:element name="parameters">
							<xsl:for-each select="param">
								<xsl:element name="parameter">
									<xsl:element name="type">
										<xsl:value-of select="type"/>
									</xsl:element>
									<xsl:element name="name">
										<xsl:value-of select="name"/>
									</xsl:element>
								</xsl:element>
							</xsl:for-each>
						</xsl:element>
					</xsl:if>
					<xsl:call-template name="typeMap">
						<xsl:with-param name="type" select="type"/>
					</xsl:call-template>
				</xsl:element>
			</xsl:for-each>
			-->
		</xsl:element>
	</xsl:template>
	<!-- ############## FUNCTION - calls type ############## -->
	<!-- cursor on doxygen/compounddef/sectiondef -->
	<xsl:template name="function">
		<xsl:for-each select="memberdef">
			<!-- test if function belongs to actual class (the location test could be deleted)  -->
			<xsl:if test="ends-with(location/@file,../../includes) and starts-with(@id,../../@id)">
				<xsl:choose>
					<!-- destructor - do nothing -->
					<xsl:when test="starts-with(name,'~')"/>
					<xsl:otherwise>
						<xsl:element name="function">
							<xsl:attribute name="virt" select="@virt"/>

							<!-- PIPAAAAAAA -->
							<xsl:attribute name="protection" select="@prot"/>
							<xsl:attribute name="static" select="@static"/>

							<xsl:attribute name="const"><xsl:choose><xsl:when test="@const='yes'"><xsl:value-of select="'true'"/></xsl:when><xsl:when test="@const='no'"><xsl:value-of select="'false'"/></xsl:when></xsl:choose></xsl:attribute>
							<!-- template attributes -->
							<xsl:if test="templateparamlist">
								<xsl:variable name="refid" select="../../@id"/>
								<xsl:attribute name="template" select="'true'"/>
								<xsl:attribute name="templateType" select="templateparamlist/param/type"/>
								<xsl:attribute name="templateDeclaration" select="templateparamlist/param/declname"/>
								<xsl:for-each select="inheritancegraph/node">
									<xsl:if test="link/@refid=$refid">
										<xsl:variable name="childnode" select="childnode/@refid"/>
										<xsl:for-each select="../node">
											<xsl:if test="@id=$childnode">
												<xsl:attribute name="templateName"><xsl:variable name="string" select="substring-before(label,' &gt;')"/><xsl:value-of select="substring-after($string,'&lt; ')"/></xsl:attribute>
											</xsl:if>
										</xsl:for-each>
									</xsl:if>
								</xsl:for-each>
							</xsl:if>
							<!-- type of the function -->
							<xsl:if test="type and type!='virtual'">
								<xsl:call-template name="type"/>
							</xsl:if>
							<xsl:element name="definition">
								<xsl:value-of select="definition"/>
							</xsl:element>
							<!-- name of the function -->
							<xsl:element name="name">
								<xsl:choose>
									<xsl:when test="contains(name,'::')">
										<xsl:value-of select="substring-after(name,'::')"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="name"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>
							<!-- parameter of the function -->
							<xsl:if test="param/type!='' or param/declname!=''">
								<xsl:element name="parameters">
									<xsl:for-each select="param">
										<xsl:element name="parameter">
											<xsl:call-template name="type"/>

											<!-- PIPAAAAAAA -->
											<xsl:if test="defval!=''">
												<xsl:element name="defval">
													<xsl:value-of select="defval"/>
												</xsl:element>
											</xsl:if>

										</xsl:element>
									</xsl:for-each>
								</xsl:element>
							</xsl:if>
						</xsl:element>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>
	<!-- ##################### TYPE - calls typeMap ##################### -->
	<!-- cursor on doxygen/compounddef/sectiondef/memberdef/param -->
	<xsl:template name="type">
		<xsl:choose>
			<!-- passed by reference -->
			<xsl:when test="contains(type,'&amp;')">
				<xsl:attribute name="passedBy" select="'reference'"/>
				<xsl:choose>
					<!-- current class is a template -->
					<xsl:when test="../../templateparamlist or ../../../templateparamlist">
						<xsl:call-template name="typeMap">
							<xsl:with-param name="type" select="substring-before(type,' &amp;')"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="type/ref">
						<xsl:call-template name="typeMap">
							<!-- don't put the select instruction into the "with-param" element,will cause multiple select of type/ref -->
							<xsl:with-param name="type">
								<xsl:value-of select="type/ref"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="typeMap">
							<xsl:with-param name="type" select="substring-before(type,' &amp;')"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<!-- passed by pointer -->
			<xsl:when test="contains(type,'*')">
				<xsl:choose>
					<xsl:when test="contains(type,'**')">
						<xsl:attribute name="passedBy" select="'PointerPointer'"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute name="passedBy" select="'pointer'"/>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:variable name="type" select="substring-before(type,' *')"/>
				<xsl:choose>
					<!-- current class is a template -->
					<xsl:when test="../../templateparamlist or ../../../templateparamlist">
						<xsl:call-template name="typeMap">
							<xsl:with-param name="type" select="substring-before(type,' *')"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="type/ref">
						<xsl:call-template name="typeMap">
							<xsl:with-param name="type">
								<xsl:value-of select="type/ref"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="typeMap">
							<xsl:with-param name="type" select="substring-before(type,' *')"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<!-- passed by value -->
			<xsl:otherwise>
				<xsl:attribute name="passedBy" select="'value'"/>
				<xsl:choose>
					<!-- current class is a template -->
					<xsl:when test="../../templateparamlist or ../../../templateparamlist">
						<xsl:call-template name="typeMap">
							<xsl:with-param name="type" select="type"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="count(type/ref)=1">
						<xsl:call-template name="typeMap">
							<xsl:with-param name="type">
								<xsl:value-of select="type/ref"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="typeMap">
							<xsl:with-param name="type" select="type"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
		<!-- name -->
		<xsl:if test="declname!=''">
			<xsl:element name="name">
				<xsl:value-of select="declname"/>
			</xsl:element>
		</xsl:if>
	</xsl:template>
	<!-- ######################## TYPEMAP ######################## -->
	<!-- cursor on doxygen/compounddef/sectiondef/memberdef/param -->
	<xsl:template name="typeMap">
		<xsl:param name="type"/>
		<xsl:if test="type!=''">
			<xsl:element name="type">
				<xsl:if test="starts-with(type,'const')">
					<xsl:attribute name="const" select="'true'"/>
				</xsl:if>

				<!-- PIPAAAA -->
				<xsl:if test="contains(type,'std::vector')">
					<xsl:attribute name="container" select="'vector'"/>
				</xsl:if>
				<xsl:if test="contains(type,'std::list')">
					<xsl:attribute name="container" select="'list'"/>
				</xsl:if>

				<xsl:if test="array">
					<xsl:attribute name="array" select="array"/>
				</xsl:if>

				<xsl:value-of select="$type"/>

			<!-- IT CONTAINED THE TYPEMAPS (Real -> float etc.) -->

			</xsl:element>
		</xsl:if>
	</xsl:template>
	<!-- ############ VARIABLE ############ -->
	<!-- cursor on doxygen/compounddef/sectiondef -->
	<xsl:template name="variable">
		<xsl:for-each select="memberdef">
			<xsl:element name="variable">

				<!-- PIPAAAAAAAA -->
				<xsl:attribute name="protection" select="@prot"/>
				<xsl:attribute name="static" select="@static"/>

				<xsl:if test="type and type!='virtual'">
					<xsl:call-template name="type"/>
				</xsl:if>
				<xsl:element name="definition">
					<xsl:value-of select="definition"/>
				</xsl:element>
				<xsl:element name="name">
					<xsl:value-of select="name"/>
				</xsl:element>
			</xsl:element>
		</xsl:for-each>
	</xsl:template>
</xsl:stylesheet>
<!--
Using the Stylesheet with version 2.0 with Saxon:
java -jar "C:/Dokumente und Einstellungen/rung/Eigene Dateien/saxon8.2/saxon8.jar" -o meta.xml all.xml input.xslt
memory problems, try this:
java -jar -Xmx512M "C:/Dokumente und Einstellungen/rung/Eigene Dateien/saxon8.2/saxon8.jar" -o meta.xml all.xml input.xslt
-->